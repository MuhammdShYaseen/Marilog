

namespace Marilog.Application.DTOs.Commands.SwiftTransfer
{
    public record CreateSwiftTransferCommand(
    string SwiftReference,
    DateOnly TransactionDate,
    int CurrencyId,
    decimal Amount,
    int? SenderCompanyId = null,
    int? ReceiverCompanyId = null,
    string? SenderBank = null,
    string? ReceiverBank = null,
    string? PaymentReference = null,
    string? RawMessage = null
);
}
