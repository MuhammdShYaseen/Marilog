namespace Marilog.Contracts.DTOs.Requests.CrewPayrollDTOs
{
    public class PayByBankTransferRequest
    {
        public int SwiftTransferId { get; set; }
        public decimal Amount { get; set; }
        public DateOnly PaidOn { get; set; }
        public string? Notes { get; set; }
    }
}
