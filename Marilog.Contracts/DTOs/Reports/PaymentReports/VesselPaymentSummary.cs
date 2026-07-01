

namespace Marilog.Contracts.DTOs.Reports.PaymentReports
{
    public class VesselPaymentSummary
    {
        public int VesselId { get; set; }
        public string? VesselName { get; set; }
        public decimal CashIn { get; set; }
        public decimal CashOut { get; set; }
        public decimal NetCashFlow { get; set; }
        public int Count { get; set; }
    }
}
