using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Reports.DocumentReports
{
    public class DocumentFilterOptions
    {
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? LastDays { get; set; }

        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public FinancialSide? Side { get; set; }
        public bool? IsDateOfPayment { get; set; }

        // ─── Multi-select (empty = all) ───
        public List<int> SupplierIds { get; set; } = [];
        public List<int> BuyerIds { get; set; } = [];
        public List<int> DocTypeIds { get; set; } = [];
        public List<int> VesselIds { get; set; } = [];
        public List<int> VoyageIds { get; set; } = [];

        // null = all
        public DocumentPaymentStatus? PaymentStatus { get; set; }

        // null = all. DueSoonDays is used only with DueStatus.DueSoon
        public DueStatus? DueStatus { get; set; }
        public int DueSoonDays { get; set; } = 10;
    }
}