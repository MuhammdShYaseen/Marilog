namespace Marilog.Application.DTOs.Reports.VoyageReports
{
    public sealed class MonthlyVoyageSummary
    {
        public int Year { get; init; }
        public int Month { get; init; }
        public int TotalVoyages { get; init; }
        public int Completed { get; init; }
        public int Underway { get; init; }
    }
}