using Marilog.Domain.Entities.SystemEntities;

namespace Marilog.Presentation.DTOs.RankDTOs
{
    public class CreateRankRequest
    {
        public string RankCode { get; set; } = null!;
        public string RankName { get; set; } = null!;
        public Department Department { get; set; }
    }
}
