using Marilog.Kernel.Enums;

namespace Marilog.Contracts.Interfaces.Services.EmailServices
{
    public interface IEmailProviderClientFactory
    {
        IEmailProviderClient GetClient(EmailProviderType providerType);
    }
}
