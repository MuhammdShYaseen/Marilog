using Marilog.Domain.Common;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
{
    /// <summary>
    /// Single payment installment inside CrewPayroll — amount in the payroll's currency.
    /// Self-contained: no links to SWIFT, voyages, offices or any other financial record.
    ///
    /// CashOnBoard  → paid to the crew member on board; Reference = voucher no. (optional)
    /// CashAtOffice → collected at the office; RecipientName + RecipientIdNumber required
    /// BankTransfer → Reference (bank transfer reference) required
    /// </summary>
    public class CrewPayrollDisbursement : Entity
    {
        public int PayrollId { get; private set; }
        public PaymentMethod Method { get; private set; }
        public decimal Amount { get; private set; }
        public DateOnly PaidOn { get; private set; }
        public DisbursementStatus Status { get; private set; } = DisbursementStatus.Pending;

        public string? RecipientName { get; private set; }
        public string? RecipientIdNumber { get; private set; }
        public string? Reference { get; private set; }

        public string? Notes { get; private set; }
        public string? CancelReason { get; private set; }

        private CrewPayrollDisbursement() { }

        internal static CrewPayrollDisbursement Create(
            int payrollId,
            PaymentMethod method,
            decimal amount,
            DateOnly paidOn,
            string? recipientName,
            string? recipientIdNumber,
            string? reference,
            string? notes)
        {
            if (!Enum.IsDefined(method))
                throw new ArgumentException($"Unknown payment method: {method}.");

            if (method == PaymentMethod.CashAtOffice &&
                (string.IsNullOrWhiteSpace(recipientName) || string.IsNullOrWhiteSpace(recipientIdNumber)))
                throw new ArgumentException(
                    "Recipient name and ID number are required for cash collected at the office.");

            if (method == PaymentMethod.BankTransfer && string.IsNullOrWhiteSpace(reference))
                throw new ArgumentException("Bank reference is required for bank transfers.");

            return new CrewPayrollDisbursement
            {
                PayrollId = payrollId,
                Method = method,
                Amount = amount,
                PaidOn = paidOn,
                RecipientName = Clean(recipientName),
                RecipientIdNumber = Clean(recipientIdNumber),
                Reference = Clean(reference),
                Notes = Clean(notes)
            };
        }

        internal void Confirm()
        {
            if (Status == DisbursementStatus.Cancelled)
                throw new InvalidOperationException("Cannot confirm a cancelled disbursement.");
            if (Status == DisbursementStatus.Confirmed)
                throw new InvalidOperationException("Disbursement is already confirmed.");

            Status = DisbursementStatus.Confirmed;
        }

        internal void Cancel(string reason)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);
            if (Status == DisbursementStatus.Cancelled)
                throw new InvalidOperationException("Disbursement is already cancelled.");

            Status = DisbursementStatus.Cancelled;
            CancelReason = reason.Trim();
        }

        private static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}