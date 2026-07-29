using Marilog.Contracts.Interfaces.Services.EmailServices;
using Marilog.Kernel.Enums;


namespace Marilog.Infrastructure.Services
{
    public class EmailProviderClientFactory : IEmailProviderClientFactory
    {
        private readonly ImapSmtpEmailProviderClient _imapSmtpClient;

        public EmailProviderClientFactory(ImapSmtpEmailProviderClient imapSmtpClient)
        {
            _imapSmtpClient = imapSmtpClient;
        }

        public IEmailProviderClient GetClient(EmailProviderType providerType) => providerType switch
        {
            EmailProviderType.Imap => _imapSmtpClient,
            EmailProviderType.MicrosoftGraph => throw new NotSupportedException(
                "Microsoft Graph provider not implemented yet."),
            EmailProviderType.Gmail => throw new NotSupportedException(
                "Gmail API provider not implemented yet."),
            _ => throw new NotSupportedException($"Unknown provider type: {providerType}")
        };
    }
}
