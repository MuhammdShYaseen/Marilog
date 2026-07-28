
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.EmailDTOs
{
    public class CreateEmailAccountRequest
    {
        public string Config { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public EmailProviderType ProviderType { get; set; }
    }
}
