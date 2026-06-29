using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Reports.DocumentReports
{
    public class DocumentFilterOptions
    {
        public int? SupplierId { get; set; }
        public int? BuyerId { get; set; }
        public int? VesselId { get; set; }
        public int? DocTypeId { get; set; }
        public bool UnpaidOnly { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? LastDays { get; set; }

        public DateOnly? FromDate { get; set; } 
        public DateOnly? ToDate { get; set; }
        public FinancialSide? Side { get; set; }
        public int? VoyageId { get; set; }
        public bool? IsDateOfPayment { get; set; }
    }
}
