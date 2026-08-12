
namespace Marilog.Infrastructure.Models.EmailNotification
{
    public class NotificationEmailMessage
    {
        public string FromEmail { get; internal set; } = string.Empty;
        public string? FromName { get; internal set; }
        public string Subject { get; internal set; } = string.Empty;
        public string HtmlBody { get; internal set; } = string.Empty;
    }
}
