namespace Marilog.Presentation.DTOs.CrewDTOs
{
    public class SignOffCrewContractRequest
    {
        public DateOnly SignOffDate { get; set; }
        public int? SignOffPort { get; set; }
    }
}
