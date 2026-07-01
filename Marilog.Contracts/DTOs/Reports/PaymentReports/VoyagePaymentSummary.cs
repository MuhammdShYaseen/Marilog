using System;


namespace Marilog.Contracts.DTOs.Reports.PaymentReports
{
    public class VoyagePaymentSummary
    {
        public int VoyageId { get; set; }
        public string? VoyageNumber { get; set; }
        public decimal CashIn { get; set; }
        public decimal CashOut { get; set; }
        public decimal NetCashFlow => CashIn - CashOut; // ← أضف هذا
        public int Count { get; set; }
    }
}
