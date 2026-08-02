

namespace Marilog.Application.Models
{
    public sealed class GoogleUserInfoResponse
    {
        public string? Email { get; set; }
        public bool VerifiedEmail { get; set; }
        public string? Name { get; set; }
    }
}
