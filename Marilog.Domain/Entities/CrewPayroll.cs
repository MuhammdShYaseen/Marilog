using Marilog.Domain.Common;
using Marilog.Domain.Events;

namespace Marilog.Domain.Entities
{
    public enum PayrollStatus { Draft, Approved, PartiallyPaid, FullyPaid, Cancelled }

    /// <summary>
    /// Aggregate Root — Monthly payroll for a single crew member.
    /// One record per (ContractId + PayrollMonth). All amounts in USD.
    ///
    /// Three disbursement methods:
    ///   PayCash()           → cash on board (deducted from Voyage.CashOnBoard)
    ///   PayAtOffice()       → cash at company office (collected by crew family member)
    ///   PayByBankTransfer() → wire transfer linked to SwiftTransfer
    /// </summary>
    public class CrewPayroll : Entity
    {
        public int ContractId { get; private set; }
        public CrewContract Contract { get; private set; } = null!;

        // ── Period ────────────────────────────────────────────────────────────────
        /// <summary>Always stored as first day of month: 2025-03-01</summary>
        public DateOnly PayrollMonth { get; private set; }
        public int WorkingDays { get; private set; }

        // ── Amounts (USD) ─────────────────────────────────────────────────────────
        public decimal BasicWage { get; private set; }
        public decimal Allowances { get; private set; }
        public decimal Deductions { get; private set; }
        public decimal GrossAmount { get; private set; }  // BasicWage + Allowances - Deductions

        // ── Status ────────────────────────────────────────────────────────────────
        public PayrollStatus Status { get; private set; } = PayrollStatus.Draft;
        public string? Notes { get; private set; }

        private readonly List<CrewPayrollDisbursement> _disbursements = new();
        public ICollection<CrewPayrollDisbursement> Disbursements
            => _disbursements.AsReadOnly();

        // ── Computed ──────────────────────────────────────────────────────────────
        public decimal TotalDisbursed =>
            _disbursements
                .Where(d => d.Status != DisbursementStatus.Cancelled)
                .Sum(d => d.Amount);

        public decimal RemainingBalance => GrossAmount - TotalDisbursed;
        public bool IsFullyPaid => RemainingBalance <= 0;

        private CrewPayroll() { }

        // ── Factory ───────────────────────────────────────────────────────────────
        public static CrewPayroll Create(
            int contractId,
            DateOnly payrollMonth,
            int workingDays,
            decimal basicWage,
            decimal allowances = 0m,
            decimal deductions = 0m,
            string? notes = null)
        {
            if (contractId <= 0) throw new ArgumentException("Invalid ContractId.");
            if (workingDays <= 0) throw new ArgumentException("WorkingDays must be positive.");
            if (basicWage < 0) throw new ArgumentException("BasicWage cannot be negative.");
            if (allowances < 0) throw new ArgumentException("Allowances cannot be negative.");
            if (deductions < 0) throw new ArgumentException("Deductions cannot be negative.");

            var gross = basicWage + allowances - deductions;
            if (gross < 0)
                throw new InvalidOperationException("GrossAmount cannot be negative.");

            return new CrewPayroll
            {
                ContractId = contractId,
                PayrollMonth = new DateOnly(payrollMonth.Year, payrollMonth.Month, 1),
                WorkingDays = workingDays,
                BasicWage = basicWage,
                Allowances = allowances,
                Deductions = deductions,
                GrossAmount = gross,
                Notes = notes
            };
        }

        // ── Update (Draft only) ───────────────────────────────────────────────────
        public void Update(
            int workingDays,
            decimal basicWage,
            decimal allowances = 0m,
            decimal deductions = 0m,
            string? notes = null)
        {
            EnsureDraft("Update");
            if (workingDays <= 0) throw new ArgumentException("WorkingDays must be positive.");
            if (basicWage < 0) throw new ArgumentException("BasicWage cannot be negative.");
            if (allowances < 0) throw new ArgumentException("Allowances cannot be negative.");
            if (deductions < 0) throw new ArgumentException("Deductions cannot be negative.");

            var gross = basicWage + allowances - deductions;
            if (gross < 0)
                throw new InvalidOperationException("GrossAmount cannot be negative.");

            WorkingDays = workingDays;
            BasicWage = basicWage;
            Allowances = allowances;
            Deductions = deductions;
            GrossAmount = gross;
            Notes = notes;
            Touch();
        }

        // ── Approval ──────────────────────────────────────────────────────────────
        public void Approve()
        {
            EnsureDraft("Approve");
            Status = PayrollStatus.Approved;
            Touch();
            AddDomainEvent(new CrewPayrollApprovedEvent(Id, ContractId, GrossAmount));
        }

        public void Cancel(string reason)
        {
            if (Status == PayrollStatus.FullyPaid)
                throw new InvalidOperationException("Cannot cancel a fully paid payroll.");
            if (Status == PayrollStatus.Cancelled)
                throw new InvalidOperationException("Payroll is already cancelled.");

            ArgumentException.ThrowIfNullOrWhiteSpace(reason);
            Status = PayrollStatus.Cancelled;
            Notes = reason;
            Touch();
        }

        // ── Disbursement methods ──────────────────────────────────────────────────

        /// <summary>Cash paid on board — deducted from Voyage.CashOnBoard.</summary>
        public CrewPayrollDisbursement PayCash(
            int voyageId,
            decimal amount,
            DateOnly paidOn,
            string? notes = null)
        {
            EnsureApproved();
            ValidateAmount(amount);
            if (voyageId <= 0) throw new ArgumentException("VoyageId is required.");

            var d = CrewPayrollDisbursement.CreateCashOnBoard(
                Id, voyageId, amount, paidOn, notes);

            return Disburse(d);
        }

        /// <summary>
        /// Cash collected at a registered company office by any person.
        /// OfficeId, recipient name and ID number are required for audit trail.
        /// </summary>
        public CrewPayrollDisbursement PayAtOffice(
            int officeId,
            decimal amount,
            DateOnly paidOn,
            string recipientName,
            string recipientIdNumber,
            string? notes = null)
        {
            EnsureApproved();
            ValidateAmount(amount);
            if (officeId <= 0) throw new ArgumentException("OfficeId is required.");
            ArgumentException.ThrowIfNullOrWhiteSpace(recipientName);
            ArgumentException.ThrowIfNullOrWhiteSpace(recipientIdNumber);

            var d = CrewPayrollDisbursement.CreateCashAtOffice(
                Id, officeId, amount, paidOn,
                recipientName, recipientIdNumber, notes);

            return Disburse(d);
        }

        /// <summary>Wire transfer linked to a SwiftTransfer record.</summary>
        public CrewPayrollDisbursement PayByBankTransfer(
            int swiftTransferId,
            decimal amount,
            DateOnly paidOn,
            string? notes = null)
        {
            EnsureApproved();
            ValidateAmount(amount);
            if (swiftTransferId <= 0) throw new ArgumentException("SwiftTransferId is required.");

            var d = CrewPayrollDisbursement.CreateBankTransfer(
                Id, swiftTransferId, amount, paidOn, notes);

            return Disburse(d);
        }

        public void ConfirmDisbursement(int disbursementId)
        {
            GetDisbursement(disbursementId).Confirm();
            Touch();
        }

        public void CancelDisbursement(int disbursementId, string reason)
        {
            GetDisbursement(disbursementId).Cancel(reason);
            RefreshStatus();
            Touch();
        }

        // ── Private helpers ───────────────────────────────────────────────────────
        private CrewPayrollDisbursement Disburse(CrewPayrollDisbursement disbursement)
        {
            _disbursements.Add(disbursement);
            RefreshStatus();
            Touch();

            if (IsFullyPaid)
                AddDomainEvent(new CrewPayrollFullyPaidEvent(Id, ContractId));

            return disbursement;
        }

        private CrewPayrollDisbursement GetDisbursement(int id) =>
            _disbursements.FirstOrDefault(x => x.Id == id)
                ?? throw new InvalidOperationException($"Disbursement {id} not found.");

        private void ValidateAmount(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.");
            if (amount > RemainingBalance)
                throw new InvalidOperationException(
                    $"Amount ${amount} exceeds remaining balance ${RemainingBalance}.");
        }

        private void EnsureDraft(string operation)
        {
            if (Status != PayrollStatus.Draft)
                throw new InvalidOperationException(
                    $"{operation} is only allowed on Draft payrolls. Current: {Status}.");
        }

        private void EnsureApproved()
        {
            if (Status is PayrollStatus.Draft or PayrollStatus.Cancelled)
                throw new InvalidOperationException(
                    $"Payroll must be Approved before disbursing. Current: {Status}.");
        }

        private void RefreshStatus()
        {
            Status = IsFullyPaid ? PayrollStatus.FullyPaid
                   : TotalDisbursed > 0 ? PayrollStatus.PartiallyPaid
                   : PayrollStatus.Approved;
        }
    }
}