using Marilog.Domain.Common;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
{
    /// <summary>
    /// Represents a single connected mailbox (IMAP/SMTP, Microsoft Graph, or Gmail).
    /// Each account has its own independent inbox and send credentials.
    /// Config (host/credentials/tokens) is stored pre-encrypted by the caller —
    /// this entity never sees or handles plaintext secrets.
    /// </summary>
    public class EmailAccount : Entity
    {
        public string DisplayName { get; private set; } = null!;
        public string EmailAddress { get; private set; } = null!;
        public EmailProviderType ProviderType { get; private set; }
        public bool IsActive { get; private set; } = true;

        // Shape depends on ProviderType:
        // Imap           -> { Host, Port, Username, Password, UseSsl, SmtpHost, SmtpPort }
        // MicrosoftGraph -> { TenantId, ClientId, ClientSecret, AccessToken, RefreshToken, ExpiresAt }
        // Gmail          -> { ClientId, ClientSecret, AccessToken, RefreshToken, ExpiresAt }
        // Always store this already AES-encrypted — same convention as AiProviders.
        public string EncryptedConfig { get; private set; } = null!;

        public DateTime? LastSyncedAt { get; private set; }

        private EmailAccount() { }

        public static EmailAccount Create(
            string displayName,
            string emailAddress,
            EmailProviderType providerType,
            string encryptedConfig)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
            ArgumentException.ThrowIfNullOrWhiteSpace(emailAddress);
            ArgumentException.ThrowIfNullOrWhiteSpace(encryptedConfig);

            return new EmailAccount
            {
                DisplayName = displayName,
                EmailAddress = emailAddress,
                ProviderType = providerType,
                EncryptedConfig = encryptedConfig,
                IsActive = true
            };
        }

        public void Rename(string displayName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
            DisplayName = displayName;
            Touch();
        }

        public void UpdateConfig(string encryptedConfig)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(encryptedConfig);
            EncryptedConfig = encryptedConfig;
            Touch();
        }

        public void Activate()
        {
            IsActive = true;
            Touch();
        }

        public void Deactivate()
        {
            IsActive = false;
            Touch();
        }

        public void MarkSynced(DateTime syncedAt)
        {
            LastSyncedAt = syncedAt;
            Touch();
        }
    }
}