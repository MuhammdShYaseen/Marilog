

namespace Marilog.Infrastructure.Interfaces.EmailNotification
{
    public interface INotificationScheduleChangeNotifier
    {
        void NotifyChanged();

        Task WaitForChangeAsync(CancellationToken cancellationToken);
    }
}
