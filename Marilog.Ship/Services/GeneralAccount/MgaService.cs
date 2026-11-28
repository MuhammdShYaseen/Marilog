using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Marilog.Kernel.Rules;
using Marilog.Ship.Domain;
using Marilog.Ship.Domain.Entities;
using Marilog.Ship.Domain.Enums;
using Marilog.Ship.Services.Unlock;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Ship.Services.GeneralAccount
{
    public class MgaService : IMgaService
    {
        private readonly IRepository<Mga> _mgaRepo;
        private readonly IRepository<ShipSetup> _setupRepo;
        private readonly IRepository<Vessel> _vesselRepo;
        private readonly IRepository<Currency> _currencyRepo;
        private readonly IRepository<CrewContract> _contractRepo;
        private readonly IRepository<Voyage> _voyageRepo;
        private readonly IRepository<Payment> _paymentRepo;
        private readonly IRepository<OwnerRemittance> _remittanceRepo;
        private readonly IRepository<CrewWithdrawal> _withdrawalRepo;
        private readonly IRepository<CrewExtra> _extraRepo;
        private readonly IRepository<UnlockLog> _unlockLogRepo;

        public MgaService(
            IRepository<Mga> mgaRepo,
            IRepository<ShipSetup> setupRepo,
            IRepository<Vessel> vesselRepo,
            IRepository<Currency> currencyRepo,
            IRepository<CrewContract> contractRepo,
            IRepository<Voyage> voyageRepo,
            IRepository<Payment> paymentRepo,
            IRepository<OwnerRemittance> remittanceRepo,
            IRepository<CrewWithdrawal> withdrawalRepo,
            IRepository<CrewExtra> extraRepo,
            IRepository<UnlockLog> unlockLogRepo)
        {
            _mgaRepo = mgaRepo;
            _setupRepo = setupRepo;
            _vesselRepo = vesselRepo;
            _currencyRepo = currencyRepo;
            _contractRepo = contractRepo;
            _voyageRepo = voyageRepo;
            _paymentRepo = paymentRepo;
            _remittanceRepo = remittanceRepo;
            _withdrawalRepo = withdrawalRepo;
            _extraRepo = extraRepo;
            _unlockLogRepo = unlockLogRepo;
        }

        private sealed record Rate(string Code, decimal ToBase);

        // ── List / period ────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<MgaListItem>> GetClosedAsync(CancellationToken ct = default)
        {
            var items = await _mgaRepo.Query().AsNoTracking()
                .Where(x => !x.IsOpening && x.Status == MgaStatus.Closed)
                .OrderByDescending(x => x.Number)
                .Select(x => new { x.Id, x.Code, x.FromDate, x.ToDate, x.MasterName, x.ClosingBalance, x.ClosedAt })
                .ToListAsync(ct);

            return items
                .Select((x, i) => new MgaListItem(x.Id, x.Code ?? "", x.FromDate, x.ToDate, x.MasterName,
                    x.ClosingBalance, x.ClosedAt, i == 0))
                .ToList();
        }

        public async Task<MgaOpenPeriod?> GetOpenPeriodAsync(CancellationToken ct = default)
        {
            var setup = await _setupRepo.Query().AsNoTracking().FirstOrDefaultAsync(ct);
            if (setup is not { IsCompleted: true }) return null;

            var last = await _mgaRepo.Query().AsNoTracking()
                .Where(x => x.Status == MgaStatus.Closed)
                .OrderByDescending(x => x.Number)
                .Select(x => new { x.ToDate })
                .FirstOrDefaultAsync(ct);
            if (last is null) return null;

            var baseCurrency = await _currencyRepo.Query().AsNoTracking()
                .Where(x => x.IsBaseCurrency)
                .Select(x => new { x.Id, x.CurrencyCode })
                .FirstOrDefaultAsync(ct)
                ?? throw new InvalidOperationException("No base currency is set.");

            // The opening balances are "as at" their day, so the first MGA starts the day after.
            var from = last.ToDate.AddDays(1);
            return new MgaOpenPeriod(from, MonthEnd(from), baseCurrency.Id, baseCurrency.CurrencyCode);
        }

        // ── Reports ──────────────────────────────────────────────────────────────

        public async Task<MgaReport> GetDraftAsync(DateOnly to, string? endReason, CancellationToken ct = default)
        {
            var period = await GetOpenPeriodAsync(ct)
                ?? throw new InvalidOperationException("Complete the initial setup first.");
            EnsureTo(period, to);

            var previous = await LastClosedAsync(ct);
            var rates = await CurrentRatesAsync(ct);
            var code = await NextCodeAsync(to, ct);

            return await BuildAsync(previous, period.From, to, rates, null, code, endReason, ct);
        }

        public async Task<MgaReport> GetReportAsync(int mgaId, CancellationToken ct = default)
        {
            var mga = await _mgaRepo.Query().AsNoTracking()
                .Include(x => x.CrewLines)
                .Include(x => x.CashLines)
                .FirstOrDefaultAsync(x => x.Id == mgaId, ct)
                ?? throw new KeyNotFoundException($"MGA {mgaId} not found.");
            if (mga.IsOpening)
                throw new InvalidOperationException("The opening balances are shown on the setup page.");

            var previous = await _mgaRepo.Query().AsNoTracking()
                .Include(x => x.CrewLines)
                .Include(x => x.CashLines)
                .Where(x => x.Status == MgaStatus.Closed && x.Number < mga.Number)
                .OrderByDescending(x => x.Number)
                .FirstAsync(ct);

            // The rates frozen with this MGA; today's only for a currency it did not use.
            var current = await CurrentRatesAsync(ct);
            var rates = new Dictionary<int, Rate>(current);
            foreach (var line in mga.CashLines.Where(x => x.RateToBase is not null))
                rates[line.CurrencyId] = new Rate(current[line.CurrencyId].Code, line.RateToBase!.Value);

            return await BuildAsync(previous, mga.FromDate, mga.ToDate, rates, mga, mga.Code ?? "", mga.EndReason, ct);
        }

        // ── Close ────────────────────────────────────────────────────────────────

        public async Task CloseAsync(CloseMgaRequest request, CancellationToken ct = default)
        {
            var period = await GetOpenPeriodAsync(ct)
                ?? throw new InvalidOperationException("Complete the initial setup first.");
            EnsureTo(period, request.To);

            if (request.To >= DateOnly.FromDateTime(DateTime.Today))
                throw new InvalidOperationException("An MGA can be closed only after its last day.");
            var isShort = request.To < period.MaxTo;
            if (isShort && string.IsNullOrWhiteSpace(request.EndReason))
                throw new InvalidOperationException("Write why the period ends before the month end.");

            var previous = await LastClosedAsync(ct);
            var rates = await CurrentRatesAsync(ct);
            var code = await NextCodeAsync(request.To, ct);
            var report = await BuildAsync(previous, period.From, request.To, rates, null, code,
                isShort ? request.EndReason : null, ct);

            // ── Physical count (mandatory) ───────────────────────────────────────
            if (request.Cash.GroupBy(x => x.CurrencyId).Any(g => g.Count() > 1))
                throw new InvalidOperationException("A currency appears more than once in the count.");
            var counted = request.Cash.ToDictionary(x => x.CurrencyId, x => x.Counted);
            foreach (var cash in report.Cash)
                if (!counted.ContainsKey(cash.CurrencyId))
                    throw new InvalidOperationException($"Enter the {cash.CurrencyCode} cash counted on board.");
            if (counted.Keys.Any(id => report.Cash.All(c => c.CurrencyId != id)))
                throw new InvalidOperationException("The count has a currency that is not on board.");

            var summary = MgaMath.Recount(report, counted, request.CountedCigarettes);
            var hasDifference = summary.CountDifference != 0 || summary.Difference != 0;
            if (hasDifference && string.IsNullOrWhiteSpace(request.Reason))
                throw new InvalidOperationException(
                    $"The count differs by {summary.CountDifference:N2} and the final check by {summary.Difference:N2} " +
                    $"{report.BaseCurrencyCode}. Write the reason to close.");

            // ── Snapshot ─────────────────────────────────────────────────────────
            var number = (await _mgaRepo.Query().MaxAsync(x => (int?)x.Number, ct) ?? 0) + 1;

            var mga = Mga.CreateClosed(number, code, period.From, request.To,
                isShort ? request.EndReason : null, report.MasterName, hasDifference ? request.Reason : null,
                summary.PreviousBalance, summary.TotalWages, summary.TotalExpenses, summary.OfficeDeductions,
                summary.TotalReceived, summary.ExchangeDifference, summary.CountDifference,
                report.ExpectedCigarettes, request.CountedCigarettes);

            foreach (var a in report.CrewAccounts)
                mga.AddClosingCrewLine(a.CrewContractId, a.WagesPaidBy, a.MonthlyWage, a.Days, a.PreviousBalance,
                    a.Wages, a.Extras, a.Cash, a.Cigarettes, a.Other, a.PaidAshore, a.Balance);
            foreach (var c in report.Cash)
                mga.AddClosingCashLine(c.CurrencyId, c.Expected, counted[c.CurrencyId], c.RateToBase);

            await _mgaRepo.AddAsync(mga, ct);

            // After a reopened MGA: keep the new figures with the unlock request.
            var edit = await _unlockLogRepo.Query()
                .Where(x => x.Target == UnlockTarget.Mga && x.UnlockedAt != null && x.DataAfter == null)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(ct);
            edit?.Finish(MgaSnapshot.ToJson(mga));

            await _mgaRepo.SaveChangesAsync(ct);
        }

        // ── Wages paid ashore ────────────────────────────────────────────────────

        public async Task<IReadOnlyList<CrewPayAshoreItem>> GetPayAshoreCrewAsync(DateOnly asOf, DateOnly wageMonth,
            CancellationToken ct = default)
        {
            var period = await GetOpenPeriodAsync(ct)
                ?? throw new InvalidOperationException("Complete the initial setup first.");
            if (asOf < period.From)
                throw new InvalidOperationException($"{asOf:dd.MM.yyyy} is inside a closed MGA.");

            var previous = await LastClosedAsync(ct);
            var rates = await CurrentRatesAsync(ct);
            var report = await BuildAsync(previous, period.From, asOf, rates, null, "", null, ct);

            var contracts = await _contractRepo.Query().AsNoTracking()
                .Where(x => x.WagesPaidBy == WagesPaidBy.Master)
                .Select(x => new { x.Id, x.MonthlyWage, x.SignOnDate, x.SignOffDate })
                .ToDictionaryAsync(x => x.Id, ct);

            return report.CrewAccounts
                .Where(a => a.WagesPaidBy == WagesPaidBy.Master && contracts.ContainsKey(a.CrewContractId))
                .Select(a =>
                {
                    var c = contracts[a.CrewContractId];
                    return new CrewPayAshoreItem(a.CrewContractId, a.CrewName, a.RankName, a.Balance,
                        CrewWageRules.MonthWage(c.MonthlyWage, c.SignOnDate, c.SignOffDate, wageMonth));
                })
                .ToList();
        }

        // ── The engine ───────────────────────────────────────────────────────────
        // One place computes every page. 'closed' = a saved MGA: its stored crew lines, cash lines
        // and totals are used; the record lists (all locked) are read again.
        private async Task<MgaReport> BuildAsync(Mga previous, DateOnly from, DateOnly to,
            IReadOnlyDictionary<int, Rate> rates, Mga? closed, string code, string? endReason, CancellationToken ct)
        {
            var setup = await _setupRepo.Query().AsNoTracking().FirstAsync(ct);
            var vesselName = await _vesselRepo.Query().AsNoTracking()
                .Where(x => x.Id == setup.VesselId)
                .Select(x => x.VesselName)
                .FirstAsync(ct);
            var baseCurrency = await _currencyRepo.Query().AsNoTracking()
                .Where(x => x.IsBaseCurrency)
                .Select(x => new { x.Id, x.CurrencyCode })
                .FirstAsync(ct);
            var masterName = closed?.MasterName ?? await GetMasterNameAsync(setup.VesselId, ct);

            var start = from.ToDateTime(TimeOnly.MinValue);
            var end = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

            Rate RateOf(int currencyId) => rates.TryGetValue(currencyId, out var r)
                ? r : throw new InvalidOperationException($"Currency {currencyId} has no exchange rate.");
            decimal ToBase(decimal amount, int currencyId) => MgaMath.ToBase(amount, RateOf(currencyId).ToBase);

            // ── Page 1: ship's movements (a stop that crosses two periods is in both) ──
            var movements = (await _voyageRepo.Query().AsNoTracking()
                    .Where(v => v.VesselID == setup.VesselId)
                    .SelectMany(v => v.Stops
                        .Where(s => (s.ArrivalDate ?? s.DepartureDate) != null
                                    && (s.ArrivalDate ?? s.DepartureDate) < end
                                    && (s.DepartureDate == null || s.DepartureDate >= start))
                        .Select(s => new MgaMovementLine(
                            v.VoyageNumber,
                            s.Port != null ? s.Port.PortName : "—",
                            s.ArrivalDate, s.DepartureDate, s.PurposeOfCall)))
                    .ToListAsync(ct))
                .OrderBy(x => x.Arrival ?? x.Departure)
                .ToList();

            // ── Page 3: payments (all methods) ───────────────────────────────────
            var payments = (await _paymentRepo.Query().AsNoTracking()
                    .Where(p => p.PaymentDate >= from && p.PaymentDate <= to && p.Document.Side == FinancialSide.Expense)
                    .Select(p => new
                    {
                        p.Id,
                        p.PaymentDate,
                        p.PaidAmount,
                        p.PaymentMethod,
                        p.Discreption,
                        p.Document.DocNumber,
                        DocTypeCode = p.Document.DocType.Code,
                        DocTypeName = p.Document.DocType.Name,
                        Supplier = p.Document.Supplier != null ? p.Document.Supplier.CompanyName : null,
                        Port = p.Document.Port != null ? p.Document.Port.PortName : null,
                        p.Document.CurrencyId
                    })
                    .ToListAsync(ct))
                .OrderBy(x => x.PaymentDate).ThenBy(x => x.Id)
                .ToList();

            var expenseLines = payments
                .Select(p => (p.DocTypeCode, Line: new MgaExpenseLine(p.PaymentDate, p.DocNumber, p.DocTypeName,
                    p.Supplier, p.Port, p.PaymentMethod, p.Discreption, RateOf(p.CurrencyId).Code,
                    p.PaidAmount, ToBase(p.PaidAmount, p.CurrencyId))))
                .ToList();
            var expenses = expenseLines.Where(x => x.DocTypeCode != ShipCodes.CigarettesDocType).Select(x => x.Line).ToList();
            var cigarettePurchases = expenseLines.Where(x => x.DocTypeCode == ShipCodes.CigarettesDocType).Select(x => x.Line).ToList();

            // ── Page 4: money from the owner ─────────────────────────────────────
            var remittances = (await _remittanceRepo.Query().AsNoTracking()
                    .Where(x => x.Date >= from && x.Date <= to)
                    .Select(x => new
                    {
                        x.Id,
                        x.Date,
                        x.Type,
                        x.Amount,
                        x.CurrencyId,
                        x.CrewContractId,
                        Port = x.Port != null ? x.Port.PortName : null,
                        x.Reference,
                        x.Notes
                    })
                    .ToListAsync(ct))
                .OrderBy(x => x.Date).ThenBy(x => x.Id)
                .ToList();

            var received = remittances
                .Where(x => x.Type != OwnerRemittanceType.CrewPaidAshore)
                .Select(x => new MgaReceivedLine(x.Date,
                    x.Notes is null ? RemittanceLabel(x.Type) : $"{RemittanceLabel(x.Type)} — {x.Notes}",
                    x.Port, x.Reference, RateOf(x.CurrencyId).Code, x.Amount, ToBase(x.Amount, x.CurrencyId)))
                // Wages paid ashore: one line per payment (all crew paid together), each share on page 5.
                .Concat(remittances
                    .Where(x => x.Type == OwnerRemittanceType.CrewPaidAshore)
                    .GroupBy(x => new { x.Date, x.CurrencyId, x.Notes, x.Reference })
                    .Select(g => new MgaReceivedLine(g.Key.Date,
                        $"{g.Key.Notes ?? "Crew wages paid ashore"} ({g.Count()} crew)",
                        null, g.Key.Reference, RateOf(g.Key.CurrencyId).Code,
                        g.Sum(x => x.Amount), g.Sum(x => ToBase(x.Amount, x.CurrencyId)))))
                .OrderBy(x => x.Date)
                .ToList();

            // ── Page 5: crew accounts ────────────────────────────────────────────
            var contracts = await _contractRepo.Query().AsNoTracking()
                .Where(x => x.VesselID == setup.VesselId)
                .Select(x => new
                {
                    x.Id,
                    Name = x.Person.FullName,
                    Rank = x.Rank.RankName,
                    x.MonthlyWage,
                    x.SignOnDate,
                    x.SignOffDate,
                    x.WagesPaidBy
                })
                .ToListAsync(ct);

            var withdrawals = await _withdrawalRepo.Query().AsNoTracking()
                .Where(x => x.Date >= start && x.Date < end)
                .Select(x => new
                {
                    x.Date,
                    x.CrewContractId,
                    x.Type,
                    DeductionName = x.DeductionType != null ? x.DeductionType.Name : null,
                    x.Amount,
                    x.CurrencyId,
                    x.Reason
                })
                .ToListAsync(ct);

            var extras = await _extraRepo.Query().AsNoTracking()
                .Where(x => x.Date >= from && x.Date <= to)
                .Select(x => new { x.Date, x.CrewContractId, x.Type, x.Amount, x.CurrencyId, x.Reason })
                .ToListAsync(ct);

            var previousLines = previous.CrewLines.ToDictionary(x => x.CrewContractId);
            var closedLines = closed?.CrewLines.ToDictionary(x => x.CrewContractId);

            var accounts = new List<MgaCrewAccount>();
            foreach (var c in contracts)
            {
                MgaCrewLine? stored = null;
                if (closedLines is not null && !closedLines.TryGetValue(c.Id, out stored)) continue;

                var myWithdrawals = withdrawals.Where(x => x.CrewContractId == c.Id).ToList();
                var myExtras = extras.Where(x => x.CrewContractId == c.Id).ToList();
                var myAshore = remittances
                    .Where(x => x.Type == OwnerRemittanceType.CrewPaidAshore && x.CrewContractId == c.Id)
                    .ToList();

                var previousBalance = stored?.PreviousBalance
                    ?? (previousLines.TryGetValue(c.Id, out var pl) ? pl.Balance : 0m);
                var days = stored?.Days ?? CrewWageRules.CountServiceDays(c.SignOnDate, c.SignOffDate, from, to);

                if (stored is null && days == 0 && previousBalance == 0
                    && myWithdrawals.Count == 0 && myExtras.Count == 0 && myAshore.Count == 0)
                    continue;

                var paidBy = stored?.WagesPaidBy ?? c.WagesPaidBy;
                var monthlyWage = stored?.MonthlyWage ?? c.MonthlyWage;
                var isMaster = paidBy == WagesPaidBy.Master;

                // Same rule as Marilog payroll (CrewWageRules) — only for crew paid through the master.
                var wages = stored?.Wages ?? (isMaster && days > 0
                    ? CrewWageRules.PeriodWage(c.MonthlyWage, c.SignOnDate, c.SignOffDate, from, to)
                    : 0m);
                var extraTotal = stored?.Extras ?? myExtras.Sum(x => ToBase(x.Amount, x.CurrencyId));
                var cash = stored?.Cash ?? myWithdrawals.Where(x => x.Type == CrewWithdrawalType.Cash).Sum(x => ToBase(x.Amount, x.CurrencyId));
                var cigarettes = stored?.Cigarettes ?? myWithdrawals.Where(x => x.Type == CrewWithdrawalType.Cigarettes).Sum(x => ToBase(x.Amount, x.CurrencyId));
                var other = stored?.Other ?? myWithdrawals.Where(x => x.Type == CrewWithdrawalType.Other).Sum(x => ToBase(x.Amount, x.CurrencyId));
                var ashore = stored?.PaidAshore ?? myAshore.Sum(x => ToBase(x.Amount, x.CurrencyId));

                var balance = stored?.Balance ?? (isMaster
                    ? previousBalance + wages + extraTotal - cash - cigarettes - other - ashore
                    : previousBalance);   // office-paid: the draws go to the office for deduction

                var entries = new List<MgaCrewEntry>();
                if (isMaster && wages != 0)
                    entries.Add(new MgaCrewEntry(from.ToDateTime(TimeOnly.MinValue), false, "Wages",
                        $"{from:dd.MM}–{to:dd.MM.yyyy}: {days} days at {monthlyWage:N2} a month",
                        baseCurrency.CurrencyCode, wages, wages));
                entries.AddRange(myExtras.Select(x => new MgaCrewEntry(x.Date.ToDateTime(TimeOnly.MinValue), false,
                    ExtraLabel(x.Type), x.Reason, RateOf(x.CurrencyId).Code, x.Amount, ToBase(x.Amount, x.CurrencyId))));
                entries.AddRange(myWithdrawals.Select(x => new MgaCrewEntry(x.Date, true,
                    x.Type switch
                    {
                        CrewWithdrawalType.Cash => "Cash",
                        CrewWithdrawalType.Cigarettes => "Cigarettes",
                        _ => x.DeductionName ?? "Other deduction"
                    },
                    x.Reason, RateOf(x.CurrencyId).Code, x.Amount, -ToBase(x.Amount, x.CurrencyId))));
                entries.AddRange(myAshore.Select(x => new MgaCrewEntry(x.Date.ToDateTime(TimeOnly.MinValue), false,
                    "Paid ashore", x.Notes ?? x.Reference, RateOf(x.CurrencyId).Code, x.Amount, -ToBase(x.Amount, x.CurrencyId))));

                accounts.Add(new MgaCrewAccount(c.Id, c.Name, c.Rank, paidBy, monthlyWage, days, previousBalance,
                    wages, extraTotal, cash, cigarettes, other, ashore, balance,
                    entries.OrderBy(x => x.Date).ToList()));
            }

            accounts = accounts
                .OrderByDescending(x => x.WagesPaidBy == WagesPaidBy.Master)
                .ThenBy(x => x.CrewName)
                .ToList();

            // ── Page 2: wages of crew paid through the master ────────────────────
            var wageLines = accounts
                .Where(a => a.WagesPaidBy == WagesPaidBy.Master && (a.Days > 0 || a.Extras != 0 || a.Other != 0))
                .Select(a => new MgaWageLine(a.CrewContractId, a.CrewName, a.RankName, a.MonthlyWage, a.Days,
                    a.Wages, a.Extras, a.Other, a.Wages + a.Extras - a.Other))
                .ToList();

            // ── Cash on board, per currency ──────────────────────────────────────
            // Cash on board (CashOnBoard, Cash) leaves the ship's cash; any other method was paid
            // ashore and must have its "Other paid ashore" line on page 4.
            var moves = new List<(int CurrencyId, decimal Amount)>();
            moves.AddRange(remittances.Where(x => x.Type == OwnerRemittanceType.CashToMaster).Select(x => (x.CurrencyId, x.Amount)));
            moves.AddRange(payments.Where(x => IsShipCash(x.PaymentMethod)).Select(x => (x.CurrencyId, -x.PaidAmount)));
            moves.AddRange(withdrawals.Where(x => x.Type == CrewWithdrawalType.Cash).Select(x => (x.CurrencyId, -x.Amount)));

            var previousCash = previous.CashLines.ToDictionary(x => x.CurrencyId);
            var currencyIds = previousCash.Keys
                .Union(moves.Select(x => x.CurrencyId))
                .Union(closed?.CashLines.Select(x => x.CurrencyId) ?? [])
                .Append(baseCurrency.Id)
                .Distinct()
                .ToList();

            var cashItems = new List<MgaCashItem>();
            foreach (var id in currencyIds)
            {
                var prev = previousCash.TryGetValue(id, out var line) ? line.Counted ?? line.Expected : 0m;
                var storedLine = closed?.CashLines.FirstOrDefault(x => x.CurrencyId == id);
                var expected = storedLine?.Expected ?? prev + moves.Where(x => x.CurrencyId == id).Sum(x => x.Amount);
                var moved = moves.Any(x => x.CurrencyId == id);

                if (storedLine is null && prev == 0 && expected == 0 && !moved && id != baseCurrency.Id) continue;
                cashItems.Add(new MgaCashItem(id, RateOf(id).Code, RateOf(id).ToBase, prev, expected, storedLine?.Counted));
            }
            cashItems = cashItems.OrderByDescending(x => x.CurrencyId == baseCurrency.Id).ThenBy(x => x.CurrencyCode).ToList();

            // Exchange difference: the previous cash at the previous rates + every move at this MGA's
            // rates should equal the expected cash at this MGA's rates. What is left is the revaluation
            // of foreign cash (and cents of rounding). A gain in value is − to the master's account.
            var previousCashBase = previous.CashLines.Sum(x =>
                MgaMath.ToBase(x.Counted ?? x.Expected, x.RateToBase ?? RateOf(x.CurrencyId).ToBase));
            var movesBase = moves.Sum(x => ToBase(x.Amount, x.CurrencyId));
            var expectedCashBase = cashItems.Sum(x => MgaMath.ToBase(x.Expected, x.RateToBase));
            var exchangeDifference = closed?.ExchangeDifference ?? previousCashBase + movesBase - expectedCashBase;

            // ── Cigarettes on board (value, base currency) ───────────────────────
            var previousCigarettes = previous.CountedCigarettes ?? previous.ExpectedCigarettes;
            var expectedCigarettes = closed?.ExpectedCigarettes ?? previousCigarettes
                + remittances.Where(x => x.Type == OwnerRemittanceType.Cigarettes).Sum(x => ToBase(x.Amount, x.CurrencyId))
                + payments.Where(x => x.DocTypeCode == ShipCodes.CigarettesDocType).Sum(x => ToBase(x.PaidAmount, x.CurrencyId))
                - payments.Where(x => x.PaymentMethod == PaymentMethod.CigarettesOnBoard).Sum(x => ToBase(x.PaidAmount, x.CurrencyId))
                - withdrawals.Where(x => x.Type == CrewWithdrawalType.Cigarettes).Sum(x => ToBase(x.Amount, x.CurrencyId));

            // ── Summary ──────────────────────────────────────────────────────────
            var crewBalances = accounts.Sum(x => x.Balance);
            var countedCashBase = cashItems.Sum(x => MgaMath.ToBase(x.Counted ?? x.Expected, x.RateToBase));

            var summary = closed is null
                ? MgaMath.Summarize(previous.ClosingBalance,
                    wageLines.Sum(x => x.Net),
                    expenses.Sum(x => x.BaseAmount),
                    accounts.Where(x => x.WagesPaidBy == WagesPaidBy.Office).Sum(x => x.Cash + x.Cigarettes),
                    received.Sum(x => x.BaseAmount),
                    exchangeDifference, crewBalances,
                    expectedCashBase, expectedCigarettes, expectedCashBase, expectedCigarettes)
                : MgaMath.Summarize(closed.PreviousBalance, closed.TotalWages, closed.TotalExpenses,
                    closed.OfficeDeductions, closed.TotalReceived, closed.ExchangeDifference, crewBalances,
                    expectedCashBase, closed.ExpectedCigarettes,
                    countedCashBase, closed.CountedCigarettes ?? closed.ExpectedCigarettes);

            return new MgaReport(closed?.Id, code, closed is not null, vesselName, masterName,
                baseCurrency.CurrencyCode, from, to, endReason, closed?.Notes, closed?.ClosedAt,
                movements, wageLines, expenses, cigarettePurchases, received, accounts, cashItems,
                previousCigarettes, expectedCigarettes, closed?.CountedCigarettes, summary);
        }

        // ── Private ──────────────────────────────────────────────────────────────

        private async Task<Mga> LastClosedAsync(CancellationToken ct) =>
            await _mgaRepo.Query().AsNoTracking()
                .Include(x => x.CrewLines)
                .Include(x => x.CashLines)
                .Where(x => x.Status == MgaStatus.Closed)
                .OrderByDescending(x => x.Number)
                .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("Complete the initial setup first.");

        // Same conversion as the setup check: (USD per unit) ÷ (USD per base unit).
        private async Task<Dictionary<int, Rate>> CurrentRatesAsync(CancellationToken ct)
        {
            var currencies = await _currencyRepo.Query().AsNoTracking()
                .IgnoreQueryFilters()   // a deleted currency can still be on an old record
                .Select(x => new { x.Id, x.CurrencyCode, x.ExchangeRate, x.IsBaseCurrency })
                .ToListAsync(ct);

            var baseRate = currencies.FirstOrDefault(x => x.IsBaseCurrency)?.ExchangeRate
                ?? throw new InvalidOperationException("No base currency is set.");
            if (baseRate <= 0)
                throw new InvalidOperationException("The base currency has no exchange rate.");

            return currencies.ToDictionary(x => x.Id,
                x => new Rate(x.CurrencyCode, x.IsBaseCurrency ? 1m : x.ExchangeRate / baseRate));
        }

        // "10/2026" from the last day; a second MGA ending in the same month is "10/2026-B".
        private async Task<string> NextCodeAsync(DateOnly to, CancellationToken ct)
        {
            var first = new DateOnly(to.Year, to.Month, 1);
            var last = MonthEnd(first);
            var sameMonth = await _mgaRepo.Query()
                .CountAsync(x => !x.IsOpening && x.Status == MgaStatus.Closed
                                 && x.ToDate >= first && x.ToDate <= last, ct);

            var code = $"{to.Month:D2}/{to.Year}";
            return sameMonth == 0 ? code : $"{code}-{(char)('A' + sameMonth)}";
        }

        private async Task<string?> GetMasterNameAsync(int vesselId, CancellationToken ct) =>
            await _contractRepo.Query().AsNoTracking()
                .Where(x => x.VesselID == vesselId && x.IsActive && x.Rank.RankCode == "MASTER")
                .Select(x => x.Person.FullName)
                .FirstOrDefaultAsync(ct);

        private static void EnsureTo(MgaOpenPeriod period, DateOnly to)
        {
            if (to < period.From || to > period.MaxTo)
                throw new InvalidOperationException(
                    $"The MGA ends between {period.From:dd.MM.yyyy} and {period.MaxTo:dd.MM.yyyy}.");
        }

        private static DateOnly MonthEnd(DateOnly date) =>
            new DateOnly(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);

        private static bool IsShipCash(PaymentMethod method) =>
            method is PaymentMethod.CashOnBoard or PaymentMethod.Cash;

        private static string RemittanceLabel(OwnerRemittanceType type) => type switch
        {
            OwnerRemittanceType.CashToMaster => "Cash to master",
            OwnerRemittanceType.Cigarettes => "Cigarettes",
            OwnerRemittanceType.CrewPaidAshore => "Wages paid ashore",
            OwnerRemittanceType.OtherPaidAshore => "Paid ashore",
            _ => type.ToString()
        };

        private static string ExtraLabel(CrewExtraType type) => type switch
        {
            CrewExtraType.Bonus => "Bonus",
            CrewExtraType.TravelTicket => "Travel ticket",
            _ => "Extra"
        };
    }
}