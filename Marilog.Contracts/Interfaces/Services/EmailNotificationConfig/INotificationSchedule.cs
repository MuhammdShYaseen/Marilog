
using Marilog.Contracts.DTOs.EmailNotificationDTOs;

namespace Marilog.Contracts.Interfaces.Services.EmailNotificationConfig
{
    public interface INotificationSchedule
    {
        Task<NotificationScheduleOptions> GetAsync(CancellationToken cancellationToken = default);

        Task SaveAsync(NotificationScheduleOptions options, CancellationToken cancellationToken = default);

        Task<DateTimeOffset?> GetNextExecutionAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
    }
}
