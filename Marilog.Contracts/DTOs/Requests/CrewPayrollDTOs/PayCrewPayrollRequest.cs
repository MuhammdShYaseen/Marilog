using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.CrewPayrollDTOs
{
    public class PayCrewPayrollRequest
    {
        public PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }
        public DateOnly PaidOn { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientIdNumber { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
    }
}