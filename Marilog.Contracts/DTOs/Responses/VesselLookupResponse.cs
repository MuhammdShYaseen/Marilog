

namespace Marilog.Contracts.DTOs.Responses
{
    public class VesselLookupResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? IMONumber { get; set; } = null!;
        public string? CompanyName { get; set; }

    }
}
