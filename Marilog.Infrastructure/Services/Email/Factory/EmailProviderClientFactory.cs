using Marilog.Application.Interfaces.Email;
using Marilog.Infrastructure.Services.Email.Google;
using Marilog.Infrastructure.Services.Email.Smtp;
using Marilog.Kernel.Enums;


namespace Marilog.Infrastructure.Services.Email.Factory
{
    public class EmailProviderClientFactory : IEmailProviderClientFactory
    {
        private readonly ImapSmtpEmailProviderClient _imapSmtpClient;
        private readonly GoogleApiEmailProviderClient _googleEmailClient;
        public EmailProviderClientFactory(ImapSmtpEmailProviderClient imapSmtpClient, GoogleApiEmailProviderClient googleEmailClient)
        {
            _imapSmtpClient = imapSmtpClient;
            _googleEmailClient = googleEmailClient;
        }

        public IEmailProviderClient GetClient(EmailProviderType providerType) => providerType switch
        {
            EmailProviderType.Imap => _imapSmtpClient,
            EmailProviderType.MicrosoftGraph => throw new NotSupportedException(
                "Microsoft Graph provider not implemented yet."),
            EmailProviderType.Gmail => _googleEmailClient,
            _ =>
            
            throw new NotSupportedException($"Unknown provider type: {providerType}")
        };
    }
}
