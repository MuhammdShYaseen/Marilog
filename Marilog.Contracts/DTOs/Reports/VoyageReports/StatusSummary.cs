using Marilog.Kernel.Enums;


namespace Marilog.Contracts.DTOs.Reports.VoyageReports
{
    public sealed class StatusSummary
    {
        public VoyageStatus Status { get; init; }
        public int Count { get; init; }
    }
}