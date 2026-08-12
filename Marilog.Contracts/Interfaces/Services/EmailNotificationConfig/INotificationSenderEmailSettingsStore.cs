

using Marilog.Contracts.DTOs.EmailNotificationDTOs;

namespace Marilog.Contracts.Interfaces.Services.EmailNotificationConfig
{
    public interface INotificationSenderEmailSettingsStore
    {
        Task<NotificationSenderEmailSettingsOptions> GetAsync(CancellationToken cancellationToken = default);

        Task UpdateAsync(NotificationSenderEmailSettingsOptions options, CancellationToken cancellationToken = default);
    }
}
