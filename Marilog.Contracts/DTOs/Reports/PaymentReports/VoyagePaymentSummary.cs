using System;


namespace Marilog.Contracts.DTOs.Reports.PaymentReports
{
    public class VoyagePaymentSummary
    {
        public int VoyageId { get; set; }
        public string? VoyageNumber { get; set; }
        public decimal CashIn { get; set; }
        public decimal CashOut { get; set; }
        public int Count { get; set; }
    }
}
