using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.DTOs.Reports.VoyageReports
{
    public sealed class VoyageReport
    {
        public IReadOnlyList<VoyageResponse> Voyages { get; init; } = [];
        public int TotalCount { get; init; }

        // إحصاءات حسب الحالة
        public IReadOnlyList<StatusSummary> StatusSummary { get; init; } = [];

        // إحصاءات حسب السفينة
        public IReadOnlyList<VesselVoyageSummary> VesselSummary { get; init; } = [];

        // إحصاءات شهرية
        public IReadOnlyList<MonthlyVoyageSummary> MonthlySummary { get; init; } = [];

        // الرحلة الحالية لكل سفينة (إن وُجدت)
        public IReadOnlyList<VoyageResponse> CurrentVoyages { get; init; } = [];
    }
}
