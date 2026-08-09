
namespace Marilog.Contracts.DTOs.Requests.EmailNotifiConfig
{
    public class UpdateNotificationRecipientRequest
    {
        public string CurrentEmail { get; set; } = null!;
        public string NewEmail { get; set; } = null!;
    }
}
