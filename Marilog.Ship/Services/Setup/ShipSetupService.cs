using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Marilog.Ship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Ship.Services.Setup
{
    public class ShipSetupService : IShipSetupService
    {
        private readonly IRepository<ShipSetup> _setupRepo;
        private readonly IRepository<Mga> _mgaRepo;
        private readonly IRepository<Vessel> _vesselRepo;
        private readonly IRepository<Company> _companyRepo;
        private readonly IRepository<Currency> _currencyRepo;
        private readonly IRepository<CrewContract> _contractRepo;

        public ShipSetupService(
            IRepository<ShipSetup> setupRepo,
            IRepository<Mga> mgaRepo,
            IRepository<Vessel> vesselRepo,
            IRepository<Company> companyRepo,
            IRepository<Currency> currencyRepo,
            IRepository<CrewContract> contractRepo)
        {
            _setupRepo = setupRepo;
            _mgaRepo = mgaRepo;
            _vesselRepo = vesselRepo;
            _companyRepo = companyRepo;
            _currencyRepo = currencyRepo;
            _contractRepo = contractRepo;
        }

        // ── Status ───────────────────────────────────────────────────────────────

        public async Task<ShipSetupStatusResponse> GetStatusAsync(CancellationToken ct = default)
        {
            var setup = await _setupRepo.Query().AsNoTracking().FirstOrDefaultAsync(ct);

            var baseCurrency = await _currencyRepo.Query().AsNoTracking()
                .Where(x => x.IsBaseCurrency)
                .Select(x => new { x.Id, x.CurrencyCode })
                .FirstOrDefaultAsync(ct);

            var hasOpening = await _mgaRepo.Query().AnyAsync(x => x.IsOpening, ct);

            if (setup is null)
                return new ShipSetupStatusResponse(false, null, null, null, null, null, null, null,
                    WagesPaidBy.Master, baseCurrency?.Id, baseCurrency?.CurrencyCode, hasOpening);

            var vessel = await _vesselRepo.Query().AsNoTracking()
                .Where(x => x.Id == setup.VesselId)
                .Select(x => new
                {
                    x.VesselName,
                    x.IMONumber,
                    x.GrossTonnage,
                    x.FlagCountryID,
                    x.VesselTypeId,
                    CompanyName = x.Company.CompanyName
                })
                .FirstAsync(ct);

            return new ShipSetupStatusResponse(
                setup.IsCompleted, setup.VesselId, vessel.VesselName, vessel.CompanyName,
                vessel.IMONumber, vessel.GrossTonnage, vessel.FlagCountryID, vessel.VesselTypeId,
                setup.DefaultWagesPaidBy, baseCurrency?.Id, baseCurrency?.CurrencyCode, hasOpening);
        }

        // ── Step 1+2: vessel, owner, base currency ───────────────────────────────

        public async Task SaveVesselAsync(SaveShipVesselRequest request, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(request.OwnerCompanyName);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.VesselName);

            var setup = await _setupRepo.Query().FirstOrDefaultAsync(ct);
            EnsureNotCompleted(setup);

            if (setup is null)
            {
                // First save: owner company → vessel → setup (each needs the Id of the one before).
                var company = Company.Create(null, null, request.OwnerCompanyName.Trim(), null);
                await _companyRepo.AddAsync(company, ct);
                await _companyRepo.SaveChangesAsync(ct);

                var vessel = Vessel.Create(company.Id, request.VesselName.Trim(), request.ImoNumber,
                    request.GrossTonnage, request.FlagCountryId, null, request.VesselTypeId);
                await _vesselRepo.AddAsync(vessel, ct);
                await _vesselRepo.SaveChangesAsync(ct);

                await _setupRepo.AddAsync(ShipSetup.Create(vessel.Id, request.DefaultWagesPaidBy), ct);
            }
            else
            {
                var vessel = await _vesselRepo.GetByIdAsync(setup.VesselId, ct)
                    ?? throw new KeyNotFoundException($"Vessel {setup.VesselId} not found.");
                var company = await _companyRepo.GetByIdAsync(vessel.CompanyID, ct)
                    ?? throw new KeyNotFoundException($"Company {vessel.CompanyID} not found.");

                company.Update(company.Website, company.RegistrationNumber, request.OwnerCompanyName.Trim(),
                    company.CountryId, company.ContactName, company.Address);
                vessel.Update(request.VesselName.Trim(), request.ImoNumber, request.GrossTonnage,
                    request.FlagCountryId, vessel.Notes, request.VesselTypeId);
                setup.Update(request.DefaultWagesPaidBy);
            }

            await _setupRepo.SaveChangesAsync(ct);
            await SetBaseCurrencyAsync(request.BaseCurrencyId, ct);
        }

        // Only one base currency is allowed by a unique index, so the old one is
        // cleared and saved before the new one is set.
        private async Task SetBaseCurrencyAsync(int currencyId, CancellationToken ct)
        {
            var target = await _currencyRepo.GetByIdAsync(currencyId, ct)
                ?? throw new KeyNotFoundException($"Currency {currencyId} not found.");
            if (target.IsBaseCurrency) return;

            var current = await _currencyRepo.Query().Where(x => x.IsBaseCurrency).ToListAsync(ct);
            foreach (var c in current) c.UnSetBase();
            await _currencyRepo.SaveChangesAsync(ct);

            target.SetAsBase();
            await _currencyRepo.SaveChangesAsync(ct);
        }

        // ── Crew ─────────────────────────────────────────────────────────────────

        public async Task SetContractWagesPaidByAsync(int crewContractId, WagesPaidBy wagesPaidBy,
            CancellationToken ct = default)
        {
            var contract = await _contractRepo.GetByIdAsync(crewContractId, ct)
                ?? throw new KeyNotFoundException($"Crew contract {crewContractId} not found.");

            contract.SetWagesPaidBy(wagesPaidBy);
            await _contractRepo.SaveChangesAsync(ct);
        }
        public async Task<IReadOnlyDictionary<int, WagesPaidBy>> GetWagesPaidByAsync(int vesselId, CancellationToken ct = default) =>
                  await _contractRepo.Query().AsNoTracking()
                    .Where(x => x.VesselID == vesselId)
                    .ToDictionaryAsync(x => x.Id, x => x.WagesPaidBy, ct);
        // ── Opening balances ─────────────────────────────────────────────────────

        public async Task<OpeningBalancesResponse?> GetOpeningAsync(CancellationToken ct = default)
        {
            var opening = await _mgaRepo.Query().AsNoTracking()
                .Include(x => x.CrewLines)
                .Include(x => x.CashLines)
                .FirstOrDefaultAsync(x => x.IsOpening, ct);

            if (opening is null) return null;

            return new OpeningBalancesResponse(
                opening.FromDate,
                opening.MasterName,
                opening.ClosingBalance,
                opening.ExpectedCigarettes,
                opening.CrewLines.Select(x => new OpeningCrewBalanceItem(x.CrewContractId, x.Balance)).ToList(),
                opening.CashLines.Select(x => new OpeningCashItem(x.CurrencyId, x.Expected)).ToList());
        }

        public async Task SaveOpeningAsync(SaveOpeningBalancesRequest request, CancellationToken ct = default)
        {
            var setup = await _setupRepo.Query().FirstOrDefaultAsync(ct)
                ?? throw new InvalidOperationException("Save the vessel first.");
            EnsureNotCompleted(setup);

            if (request.CrewBalances.GroupBy(x => x.CrewContractId).Any(g => g.Count() > 1))
                throw new InvalidOperationException("A crew member appears more than once.");
            if (request.Cash.GroupBy(x => x.CurrencyId).Any(g => g.Count() > 1))
                throw new InvalidOperationException("A currency appears more than once.");

            var contractIds = request.CrewBalances.Select(x => x.CrewContractId).ToList();
            var validContracts = await _contractRepo.Query()
                .CountAsync(x => contractIds.Contains(x.Id) && x.VesselID == setup.VesselId, ct);
            if (validContracts != contractIds.Count)
                throw new InvalidOperationException("Some crew contracts do not belong to this vessel.");

            var currencyIds = request.Cash.Select(x => x.CurrencyId).ToList();
            if (await _currencyRepo.Query().CountAsync(x => currencyIds.Contains(x.Id), ct) != currencyIds.Count)
                throw new InvalidOperationException("Some currencies were not found.");

            var masterName = await GetMasterNameAsync(setup.VesselId, ct);

            var opening = await _mgaRepo.Query()
                .Include(x => x.CrewLines)
                .Include(x => x.CashLines)
                .FirstOrDefaultAsync(x => x.IsOpening, ct);

            if (opening is null)
            {
                opening = Mga.CreateOpening(request.AsOf, masterName,
                    request.MasterAccountBalance, request.CigarettesOnBoard);
                await _mgaRepo.AddAsync(opening, ct);
            }
            else
            {
                opening.UpdateOpening(request.AsOf, masterName,
                    request.MasterAccountBalance, request.CigarettesOnBoard);
            }

            foreach (var line in opening.CrewLines.Where(x => !contractIds.Contains(x.CrewContractId)).ToList())
                opening.RemoveOpeningCrewBalance(line.CrewContractId);
            foreach (var item in request.CrewBalances)
                opening.SetOpeningCrewBalance(item.CrewContractId, item.Balance);

            foreach (var line in opening.CashLines.Where(x => !currencyIds.Contains(x.CurrencyId)).ToList())
                opening.RemoveOpeningCash(line.CurrencyId);
            foreach (var item in request.Cash)
                opening.SetOpeningCash(item.CurrencyId, item.Amount);

            await _mgaRepo.SaveChangesAsync(ct);
        }

        public async Task<OpeningCheckResponse> CheckOpeningAsync(CancellationToken ct = default)
        {
            var opening = await _mgaRepo.Query().AsNoTracking()
                .Include(x => x.CrewLines)
                .Include(x => x.CashLines)
                .FirstOrDefaultAsync(x => x.IsOpening, ct)
                ?? throw new InvalidOperationException("Opening balances have not been entered.");

            var currencies = await _currencyRepo.Query().AsNoTracking()
                .Select(x => new { x.Id, x.CurrencyCode, x.ExchangeRate, x.IsBaseCurrency })
                .ToListAsync(ct);

            var baseCurrency = currencies.FirstOrDefault(x => x.IsBaseCurrency)
                ?? throw new InvalidOperationException("No base currency is set.");

            // Cash in base currency: amount × (USD per unit) ÷ (USD per base unit).
            var cash = opening.CashLines.Sum(line =>
            {
                var rate = currencies.First(x => x.Id == line.CurrencyId).ExchangeRate;
                return Math.Round(line.Expected * rate / baseCurrency.ExchangeRate, 2, MidpointRounding.AwayFromZero);
            });

            var crewTotal = opening.CrewLines.Sum(x => x.Balance);
            var difference = opening.ClosingBalance - crewTotal + cash + opening.ExpectedCigarettes;

            return new OpeningCheckResponse(baseCurrency.CurrencyCode, opening.ClosingBalance,
                crewTotal, cash, opening.ExpectedCigarettes, difference);
        }

        // ── Complete ─────────────────────────────────────────────────────────────

        public async Task CompleteAsync(string? differenceReason, CancellationToken ct = default)
        {
            var setup = await _setupRepo.Query().FirstOrDefaultAsync(ct)
                ?? throw new InvalidOperationException("Save the vessel first.");
            EnsureNotCompleted(setup);

            var check = await CheckOpeningAsync(ct);
            if (check.Difference != 0 && string.IsNullOrWhiteSpace(differenceReason))
                throw new InvalidOperationException(
                    $"The opening balances differ by {check.Difference:N2} {check.BaseCurrencyCode}. Write the reason to continue.");

            var opening = await _mgaRepo.Query().FirstAsync(x => x.IsOpening, ct);

            opening.CloseOpening(check.Difference != 0 ? differenceReason : null);
            setup.Complete();
            await _setupRepo.SaveChangesAsync(ct);
        }

        // ── Private ──────────────────────────────────────────────────────────────

        private async Task<string?> GetMasterNameAsync(int vesselId, CancellationToken ct) =>
            await _contractRepo.Query()
                .Where(x => x.VesselID == vesselId && x.IsActive && x.Rank.RankCode == "MASTER")
                .Select(x => x.Person.FullName)
                .FirstOrDefaultAsync(ct);

        private static void EnsureNotCompleted(ShipSetup? setup)
        {
            if (setup is { IsCompleted: true })
                throw new InvalidOperationException("The initial setup is completed and locked.");
        }
    }
}