namespace Marilog.Contracts.DTOs.Reports.CrewContractReports
{
    public sealed class CrewContractFilterOptions
    {
        public int? VesselId { get; init; }
        public int? PersonId { get; init; }
        public bool? IsActive { get; init; }
        public string? RankCode { get; init; }
        public bool OnlyMasters { get; init; }  // ✅ bool بدون ? — false افتراضياً
        public DateOnly? OnDate { get; init; }
    }
}
