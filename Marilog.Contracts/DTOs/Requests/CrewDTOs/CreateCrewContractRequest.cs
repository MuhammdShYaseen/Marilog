namespace Marilog.Contracts.DTOs.Requests.CrewDTOs
{
    public class CreateCrewContractRequest
    {
        public int PersonId { get; set; }
        public int VesselId { get; set; }
        public int RankId { get; set; }
        public decimal MonthlyWage { get; set; }
        public DateOnly SignOnDate { get; set; }
        public int? SignOnPort { get; set; }
        public string? Notes { get; set; }

        public int DurationInMonth {  get; set; }
    }
}
