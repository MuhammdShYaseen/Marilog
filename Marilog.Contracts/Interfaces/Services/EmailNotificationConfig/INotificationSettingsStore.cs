using Marilog.Contracts.DTOs.EmailNotificationDTOs;

namespace Marilog.Contracts.Interfaces.Services.EmailNotificationConfig
{
    public interface INotificationSettingsStore
    {
        Task<NotificationSettingsOptions> GetAsync(CancellationToken cancellationToken = default);

        Task UpdateAsync(NotificationSettingsOptions options, CancellationToken cancellationToken = default);
    }
}
