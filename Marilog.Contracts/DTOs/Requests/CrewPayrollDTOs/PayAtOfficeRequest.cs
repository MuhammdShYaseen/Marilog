namespace Marilog.Contracts.DTOs.Requests.CrewPayrollDTOs
{
    public class PayAtOfficeRequest
    {
        public int OfficeId { get; set; }
        public decimal Amount { get; set; }
        public DateOnly PaidOn { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientIdNumber { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
