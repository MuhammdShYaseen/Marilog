

using System.Text.Json.Serialization;

namespace Marilog.Application.Models
{
    public sealed class GoogleUserInfoResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("verified_email")]
        public bool VerifiedEmail { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("picture")]
        public string Picture { get; set; } = "";
    }
}
