using Marilog.Contracts.Options;

namespace Marilog.Contracts.Interfaces.Services.EmailNotificationConfig
{
    public interface INotificationSettingsStore
    {
        Task<NotificationSettingsOptions> GetAsync(CancellationToken cancellationToken = default);

        Task UpdateAsync(NotificationSettingsOptions options, CancellationToken cancellationToken = default);
    }
}
