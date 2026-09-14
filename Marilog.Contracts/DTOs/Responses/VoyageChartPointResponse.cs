

namespace Marilog.Contracts.DTOs.Responses
{
    public class VoyageChartPointResponse
    {
        public DateTime PeriodStart { get; set; }
        public string? PeriodLabel { get; set; }
        public int VoyageCount { get; set; }
        public decimal CargoMT { get; set; }
    }
}
