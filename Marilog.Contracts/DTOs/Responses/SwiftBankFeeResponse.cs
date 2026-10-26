using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public class SwiftBankFeeResponse
    {
        public int Id { get; set; }
        public SwiftFeeStage Stage { get; set; }
        public int? BankId { get; set; }
        public string? BankName { get; set; }
        public string? BankSwiftBic { get; set; }
        public decimal Amount { get; set; }
        public bool IsDeducted { get; set; }
        public DateOnly FeeDate { get; set; }
        public string? Description { get; set; }
    }
}