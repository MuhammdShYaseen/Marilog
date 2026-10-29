
namespace Marilog.Contracts.DTOs.Responses
{
    public class RefreshRatesResponse
    {
        public int UpdatedCount { get; set; }
        public List<string> NotFoundCodes { get; set; } = new();
        public bool IsThrottled { get; set; }
        public DateTime? NextAllowedAtUtc { get; set; }
    }
}
