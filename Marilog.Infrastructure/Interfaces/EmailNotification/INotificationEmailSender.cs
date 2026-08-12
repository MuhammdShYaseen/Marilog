using Marilog.Infrastructure.Models.EmailNotification;

namespace Marilog.Infrastructure.Interfaces.EmailNotification
{
    public interface INotificationEmailSender
    {
        Task SendAsync(NotificationEmailMessage message, IReadOnlyCollection<string> recipients, CancellationToken cancellationToken = default);
    }
}
