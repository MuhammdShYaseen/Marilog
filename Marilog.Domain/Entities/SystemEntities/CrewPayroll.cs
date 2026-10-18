using Marilog.Domain.Common;
using Marilog.Domain.Events;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
{
    /// <summary>
    /// Aggregate Root — monthly payroll for one crew contract.
    /// One record per (ContractId + PayrollMonth). All amounts in USD.
    ///
    /// Lifecycle : Draft → Approved → PartiallyPaid → FullyPaid
    ///             Cancelled: only when no active (Pending/Confirmed) disbursements exist.
    /// BasicWage : MonthlyWage × ServiceDays ÷ DaysInMonth (sign-on and sign-off days both counted)
    /// Gross     : BasicWage + Allowances − Deductions
    /// Pending + Confirmed disbursements reserve the balance; FullyPaid requires all of them Confirmed.
    /// </summary>
    public class CrewPayroll : Entity
    {
        public int ContractId { get; private set; }
        public CrewContract Contract { get; private set; } = null!;

        // ── Period ────────────────────────────────────────────────────────────────
        public DateOnly PayrollMonth { get; private set; }   // always 1st of month
        public int WorkingDays { get; private set; }

        // ── Amounts (USD) ─────────────────────────────────────────────────────────
        public decimal BasicWage { get; private set; }
        public decimal Allowances { get; private set; }
        public decimal Deductions { get; private set; }
        public decimal GrossAmount { get; private set; }
        public int CurrencyId { get; private set; }
        public Currency Currency { get; private set; } = null!;
        // ── Status ────────────────────────────────────────────────────────────────
        public PayrollStatus Status { get; private set; } = PayrollStatus.Draft;
        public string? Notes { get; private set; }
        public string? CancelReason { get; private set; }

        private readonly List<CrewPayrollDisbursement> _disbursements = new();
        public ICollection<CrewPayrollDisbursement> Disbursements => _disbursements.AsReadOnly();

        // ── Computed (in-memory — requires Disbursements loaded) ──────────────────
        public decimal TotalDisbursed => ActiveDisbursements.Sum(d => d.Amount);
        public decimal RemainingBalance => GrossAmount - TotalDisbursed;
        public bool IsFullyPaid => Status == PayrollStatus.FullyPaid;

        private IEnumerable<CrewPayrollDisbursement> ActiveDisbursements =>
            _disbursements.Where(d => d.Status != DisbursementStatus.Cancelled);

        private CrewPayroll() { }

        // ── Factory ───────────────────────────────────────────────────────────────
        public static CrewPayroll Create(CrewContract contract, DateOnly payrollMonth,
                                         int currencyId, decimal allowances = 0m,
                                         decimal deductions = 0m, string? notes = null)
        {
            ArgumentNullException.ThrowIfNull(contract);
            if (contract.Id <= 0)
                throw new ArgumentException("Contract must be saved before creating a payroll.");

            var payroll = new CrewPayroll
            {
                ContractId = contract.Id,
                PayrollMonth = new DateOnly(payrollMonth.Year, payrollMonth.Month, 1)
            };
            payroll.Calculate(contract, currencyId, allowances, deductions, notes);
            return payroll;
        }

        public void Update(CrewContract contract, int currencyId,
                           decimal allowances = 0m, decimal deductions = 0m, string? notes = null)
        {
            EnsureStatus(PayrollStatus.Draft, "Update");
            ArgumentNullException.ThrowIfNull(contract);
            if (contract.Id != ContractId)
                throw new InvalidOperationException("Contract does not belong to this payroll.");

            Calculate(contract, currencyId, allowances, deductions, notes);
            Touch();
        }

        // ── Approval / Cancellation ───────────────────────────────────────────────
        public void Approve(CrewContract contract, DateOnly today)
        {
            EnsureStatus(PayrollStatus.Draft, "Approve");
            ArgumentNullException.ThrowIfNull(contract);
            if (contract.Id != ContractId)
                throw new InvalidOperationException("Contract does not belong to this payroll.");

            var lastDay = PayrollMonth.AddMonths(1).AddDays(-1);
            var signedOffInMonth = contract.SignOffDate is { } off && off <= lastDay && off <= today;
            if (today < lastDay && !signedOffInMonth)
                throw new InvalidOperationException(
                    $"{PayrollMonth:MMM yyyy} payroll can be approved from {lastDay:dd MMM yyyy}, " +
                    "or earlier once the crew member has signed off.");

            var (serviceDays, basicWage) = ComputeBasicWage(contract, PayrollMonth);
            if (serviceDays != WorkingDays || basicWage != BasicWage)
                throw new InvalidOperationException(
                    $"The contract changed after this draft was calculated " +
                    $"({WorkingDays} days / {BasicWage:N2} → {serviceDays} days / {basicWage:N2}). " +
                    "Open the draft and save it to recalculate, then approve.");

            Status = PayrollStatus.Approved;
            AddDomainEvent(new CrewPayrollApprovedEvent(Id, ContractId, GrossAmount));
            RefreshStatus();
            Touch();
        }

        public void Cancel(string reason)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);
            if (Status == PayrollStatus.Cancelled)
                throw new InvalidOperationException("Payroll is already cancelled.");
            if (ActiveDisbursements.Any())
                throw new InvalidOperationException(
                    "Cannot cancel a payroll with active disbursements. Cancel the disbursements first.");

            Status = PayrollStatus.Cancelled;
            CancelReason = reason.Trim();
            Touch();
        }

        // ── Disbursements ─────────────────────────────────────────────────────────
        public CrewPayrollDisbursement Pay(PaymentMethod method, decimal amount, DateOnly paidOn, string? recipientName = null,
                                           string? recipientIdNumber = null, string? reference = null, string? notes = null)
        {
            EnsureCanDisburse(amount);

            var disbursement = CrewPayrollDisbursement.Create(
                Id, method, amount, paidOn, recipientName, recipientIdNumber, reference, notes);

            _disbursements.Add(disbursement);
            RefreshStatus();
            Touch();
            return disbursement;
        }

        public void ConfirmDisbursement(int disbursementId)
        {
            GetDisbursement(disbursementId).Confirm();
            RefreshStatus();
            Touch();
        }

        public void CancelDisbursement(int disbursementId, string reason)
        {
            GetDisbursement(disbursementId).Cancel(reason);
            RefreshStatus();
            Touch();
        }

        // ── Private: calculation ──────────────────────────────────────────────────
        private const int DaysPerMonth = 30;

        private void Calculate(CrewContract contract, int currencyId,
    decimal allowances, decimal deductions, string? notes)
        {
            if (currencyId <= 0) throw new ArgumentException("CurrencyId is required.");
            EnsureMoney(allowances, nameof(Allowances));
            EnsureMoney(deductions, nameof(Deductions));

            var (serviceDays, basicWage) = ComputeBasicWage(contract, PayrollMonth);

            var gross = basicWage + allowances - deductions;
            if (gross < 0)
                throw new InvalidOperationException(
                    $"Deductions ({deductions:N2}) exceed basic wage plus allowances ({basicWage + allowances:N2}).");

            CurrencyId = currencyId;
            WorkingDays = serviceDays;
            BasicWage = basicWage;
            Allowances = allowances;
            Deductions = deductions;
            GrossAmount = gross;
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        }

        /// <summary>Full calendar month → full MonthlyWage; otherwise days × (MonthlyWage ÷ 30).</summary>
        private static (int ServiceDays, decimal BasicWage) ComputeBasicWage(CrewContract contract, DateOnly month)
        {
            if (contract.MonthlyWage < 0)
                throw new InvalidOperationException($"Contract {contract.Id} has a negative MonthlyWage.");

            var serviceDays = CountServiceDays(contract.SignOnDate, contract.SignOffDate, month);
            if (serviceDays == 0)
                throw new InvalidOperationException(
                    $"Contract {contract.Id} has no service days in {month:yyyy-MM}.");

            var isFullMonth = serviceDays == DateTime.DaysInMonth(month.Year, month.Month);
            var basicWage = Math.Round(
                isFullMonth ? contract.MonthlyWage : contract.MonthlyWage * serviceDays / DaysPerMonth,
                2, MidpointRounding.AwayFromZero);

            return (serviceDays, basicWage);
        }

        private static string NormalizeCurrency(string currencyCode)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);
            var code = currencyCode.Trim().ToUpperInvariant();
            if (code.Length != 3 || !code.All(char.IsAsciiLetter))
                throw new ArgumentException($"'{currencyCode}' is not a valid 3-letter currency code.");
            return code;
        }

        private static int CountServiceDays(DateOnly signOn, DateOnly? signOff, DateOnly month)
        {
            var firstDay = new DateOnly(month.Year, month.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var start = signOn > firstDay ? signOn : firstDay;
            var end = signOff is { } off && off < lastDay ? off : lastDay;

            return end < start ? 0 : end.DayNumber - start.DayNumber + 1;
        }

        // ── Private: disbursement helpers ─────────────────────────────────────────
        private CrewPayrollDisbursement Add(CrewPayrollDisbursement disbursement)
        {
            _disbursements.Add(disbursement);
            RefreshStatus();
            Touch();
            return disbursement;
        }

        private CrewPayrollDisbursement GetDisbursement(int id) =>
            _disbursements.FirstOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Disbursement {id} not found in payroll {Id}.");

        private void EnsureCanDisburse(decimal amount)
        {
            if (Status is not (PayrollStatus.Approved or PayrollStatus.PartiallyPaid))
                throw new InvalidOperationException(
                    $"Disbursements are only allowed on Approved or PartiallyPaid payrolls. Current: {Status}.");

            EnsureMoney(amount, "Amount");
            if (amount == 0) throw new ArgumentException("Amount must be positive.");
            if (amount > RemainingBalance)
                throw new InvalidOperationException(
                    $"Amount {amount:N2} {Currency.CurrencyCode} exceeds remaining balance {RemainingBalance:N2} {Currency.CurrencyCode}.");
        }

        private void RefreshStatus()
        {
            var active = ActiveDisbursements.ToList();

            var newStatus =
                RemainingBalance <= 0 && active.All(d => d.Status == DisbursementStatus.Confirmed)
                    ? PayrollStatus.FullyPaid
                : active.Count > 0
                    ? PayrollStatus.PartiallyPaid
                    : PayrollStatus.Approved;

            if (newStatus == Status) return;

            Status = newStatus;
            if (newStatus == PayrollStatus.FullyPaid)
                AddDomainEvent(new CrewPayrollFullyPaidEvent(Id, ContractId));
        }

        // ── Private: guards ───────────────────────────────────────────────────────
        private static void EnsureMoney(decimal value, string name)
        {
            if (value < 0)
                throw new ArgumentException($"{name} cannot be negative.");
            if (decimal.Round(value, 2) != value)
                throw new ArgumentException($"{name} cannot have more than 2 decimal places.");
        }

        private void EnsureStatus(PayrollStatus required, string operation)
        {
            if (Status != required)
                throw new InvalidOperationException(
                    $"{operation} is only allowed on {required} payrolls. Current: {Status}.");
        }
    }
}