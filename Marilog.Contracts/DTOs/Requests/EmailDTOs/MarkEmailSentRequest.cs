namespace Marilog.Contracts.DTOs.Requests.EmailDTOs
{
    public class MarkEmailSentRequest
    {
        public DateTime SentAt { get; set; }
        public string? ExternalId { get; set; }
    }
}
