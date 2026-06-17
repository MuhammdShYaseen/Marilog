
namespace Marilog.Contracts.DTOs.Responses
{
    public class PersonSeaServiceResponse
    {
        public int Index { get; set; }
        public int RankId { get; set; }
        public int ExperienceInMonths { get; set; }
        public decimal? VesselSizeInMT { get; set; }
        public string? RankName { get; set; }
    }
}
