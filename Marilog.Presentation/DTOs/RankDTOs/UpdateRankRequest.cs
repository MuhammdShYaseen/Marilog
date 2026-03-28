using Marilog.Domain.Entities;

namespace Marilog.Presentation.DTOs.RankDTOs
{
    public class UpdateRankRequest
    {
        public string RankCode { get; set; } = null!;
        public string RankName { get; set; } = null!;
        public Department Department { get; set; }
    }
}
