

using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Reports.PaymentReports
{
    public class PaymentReportRow
    {
        public int PaymentId { get; set; }
        public DateOnly PaymentDate { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PaidAmountBase { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public int? SwiftTransferId { get; set; }
        public int DocumentId { get; set; }
        public string? DocNumber { get; set; }
        public DateOnly DocDate { get; set; }
        public int DocTypeId { get; set; }
        public string? DocTypeName { get; set; }
        public FinancialSide Side { get; set; }
        public string? CurrencyCode { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int? BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public int? VesselId { get; set; }
        public string? VesselName { get; set; }
        public int? VoyageId { get; set; }
        public string? VoyageNumber { get; set; }
    }
}
