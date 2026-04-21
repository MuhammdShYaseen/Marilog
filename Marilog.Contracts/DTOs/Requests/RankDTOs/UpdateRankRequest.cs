using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.RankDTOs
{
    public class UpdateRankRequest
    {
        public string RankCode { get; set; } = null!;
        public string RankName { get; set; } = null!;
        public Department Department { get; set; }
    }
}
