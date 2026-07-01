

namespace Marilog.Contracts.DTOs.Reports.PaymentReports
{
    public class PaymentsReport
    {
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
        public List<PaymentReportRow> Payments { get; set; } = [];
        public decimal CashIn { get; set; }
        public decimal CashOut { get; set; }
        public decimal NetCashFlow { get; set; }
        public List<MonthlyPaymentSummary> MonthlySummary { get; set; } = [];
        public List<PaymentMethodSummary> MethodSummary { get; set; } = [];
        public List<VesselPaymentSummary> VesselSummary { get; set; } = [];
        public List<SupplierPaymentSummary> SupplierSummary { get; set; } = [];
        public List<BuyerPaymentSummary> BuyerSummary { get; set; } = [];
        public List<VoyagePaymentSummary> VoyageSummary { get; set; } = [];
        public int Count { get; set; }
        public string? BaseCurrencyCode { get; set; }
    }
}
