namespace Marilog.Contracts.DTOs.Responses
{
    public class VoyageStopResponse
    {
        public int StopOrder { get; set; }
        public int? PortId { get; set; }
        public string PortName { get; set; } = null!;
        public DateTime? ArrivalDate { get; set; }
        public DateTime? DepartureDate { get; set; }
        public string? PurposeOfCall { get; set; }
        public string? Notes { get; set; }
    }
}
