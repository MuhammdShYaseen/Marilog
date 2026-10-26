using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.SwiftTransferDTOs
{
    public class SwiftBankFeeRequest
    {
        public SwiftFeeStage Stage { get; set; }
        public int? BankId { get; set; }
        public decimal Amount { get; set; }
        public bool IsDeducted { get; set; }
        public DateOnly FeeDate { get; set; }
        public string? Description { get; set; }
    }
}