

using Marilog.Contracts.Options;

namespace Marilog.Contracts.Interfaces.Services.EmailNotificationConfig
{
    public interface INotificationSenderEmailSettingsStore
    {
        Task<NotificationSenderEmailSettingsOptions> GetAsync(CancellationToken cancellationToken = default);

        Task UpdateAsync(NotificationSenderEmailSettingsOptions options, CancellationToken cancellationToken = default);
    }
}
