

namespace Marilog.Contracts.DTOs.EmailNotificationDTOs
{
    public class NotificationScheduleOptions
    {
        public bool Enabled { get; set; } = true;

        public TimeOnly ExecutionTime { get; set; } = new(8, 26, 20);

        public string TimeZoneId { get; set; } = "UTC";
    }
}
