namespace Marilog.Contracts.DTOs.Requests.CrewPayrollDTOs
{
    public class PayCashRequest
    {
        public int VoyageId { get; set; }
        public decimal Amount { get; set; }
        public DateOnly PaidOn { get; set; }
        public string? Notes { get; set; }
    }
}
