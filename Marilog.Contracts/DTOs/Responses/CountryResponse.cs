
namespace Marilog.Contracts.DTOs.Responses
{
    public class CountryResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
