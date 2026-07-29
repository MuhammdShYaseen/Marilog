using Marilog.Kernel.Enums;

namespace Marilog.Application.Interfaces.Email
{
    public interface IEmailProviderClientFactory
    {
        IEmailProviderClient GetClient(EmailProviderType providerType);
    }
}
