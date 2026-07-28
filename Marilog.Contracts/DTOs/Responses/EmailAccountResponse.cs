
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public class EmailAccountResponse
    {
        public string? DisplayName { get;  set; }
        public string? EmailAddress { get;  set; } 
        public EmailProviderType ProviderType { get;  set; }
        public DateTime? LastSyncedAt { get;  set; }
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }
}
