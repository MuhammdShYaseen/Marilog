using Marilog.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Entities
{
    public enum PaymentMethod
    {
        CashOnBoard,   // نقداً على ظهر الباخرة   — VoyageId مطلوب
        CashAtOffice,  // نقداً في مكتب الشركة    — City + Country + بيانات المستلم
        BankTransfer   // حوالة بنكية              — SwiftTransferId مطلوب
    }

    public enum DisbursementStatus { Pending, Confirmed, Cancelled }

    /// <summary>
    /// Owned entity — single payment installment inside CrewPayroll.
    /// All amounts in USD.
    ///
    /// CashOnBoard  → VoyageId required
    /// 
    /// BankTransfer → SwiftTransferId required
    /// </summary>
    public class CrewPayrollDisbursement : Entity
    {
        public int PayrollId { get; private set; }
        public PaymentMethod Method { get; private set; }
        public decimal Amount { get; private set; }   // USD
        public DateOnly PaidOn { get; private set; }
        public DisbursementStatus Status { get; private set; } = DisbursementStatus.Pending;

        // ── CashOnBoard ───────────────────────────────────────────────────────────
        public int? VoyageId { get; private set; }
        public Voyage? Voyage { get; private set; }

        // ── CashAtOffice ──────────────────────────────────────────────────────────
        public int? OfficeId { get; private set; }
        public Office? Office { get; private set; }
        public string? RecipientName { get; private set; }   // اسم المستلم
        public string? RecipientIdNumber { get; private set; }   // رقم الهوية / الجواز

        // ── BankTransfer ──────────────────────────────────────────────────────────
        public int? SwiftTransferId { get; private set; }
        public SwiftTransfer? SwiftTransfer { get; private set; }

        public string? Notes { get; private set; }
        public string? CancelReason { get; private set; }

        private CrewPayrollDisbursement() { }

        internal static CrewPayrollDisbursement CreateCashOnBoard(
            int payrollId,
            int voyageId,
            decimal amount,
            DateOnly paidOn,
            string? notes = null)
        {
            return new CrewPayrollDisbursement
            {
                PayrollId = payrollId,
                Method = PaymentMethod.CashOnBoard,
                Amount = amount,
                PaidOn = paidOn,
                VoyageId = voyageId,
                Notes = notes
            };
        }

        internal static CrewPayrollDisbursement CreateCashAtOffice(int payrollId, int officeId, decimal amount, DateOnly paidOn,
                                                                   string recipientName, string recipientIdNumber, string? notes = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(recipientName);
            ArgumentException.ThrowIfNullOrWhiteSpace(recipientIdNumber);

            return new CrewPayrollDisbursement
            {
                PayrollId = payrollId,
                Method = PaymentMethod.CashAtOffice,
                Amount = amount,
                PaidOn = paidOn,
                OfficeId = officeId,
                RecipientName = recipientName,
                RecipientIdNumber = recipientIdNumber,
                Notes = notes
            };
        }

        internal static CrewPayrollDisbursement CreateBankTransfer(
            int payrollId,
            int swiftTransferId,
            decimal amount,
            DateOnly paidOn,
            string? notes = null)
        {
            return new CrewPayrollDisbursement
            {
                PayrollId = payrollId,
                Method = PaymentMethod.BankTransfer,
                Amount = amount,
                PaidOn = paidOn,
                SwiftTransferId = swiftTransferId,
                Notes = notes
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
            if (Status == DisbursementStatus.Confirmed)
                throw new InvalidOperationException("Cannot cancel a confirmed disbursement.");
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);
            Status = DisbursementStatus.Cancelled;
            CancelReason = reason;
        }
    }
}
