using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public class RankResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public Department Department { get; set; }   // enum instead of string
        public bool IsActive { get; set; }
    }
}
