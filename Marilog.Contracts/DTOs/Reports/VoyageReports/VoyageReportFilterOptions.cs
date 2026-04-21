using Marilog.Kernel.Enums;


namespace Marilog.Contracts.DTOs.Reports.VoyageReports
{
    public sealed class VoyageReportFilterOptions
    {
        public int? Id { get; init; }
        public int? VesselId { get; init; }
        public DateOnly? Month { get; init; }
        public VoyageStatus? Status { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public bool OnlyCurrent { get; init; }        // رحلات UNDERWAY فقط
        public bool IncludeStops { get; init; }       // هل نجلب المحطات؟
    }
}
