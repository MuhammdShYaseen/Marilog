namespace Marilog.Contracts.DTOs.Reports.VoyageReports
{
    public sealed class VesselVoyageSummary
    {
        public int VesselId { get; init; }
        public string VesselName { get; init; } = string.Empty;
        public int TotalVoyages { get; init; }
        public int Underway { get; init; }
        public int Completed { get; init; }
        public int Planned { get; init; }
    }
}