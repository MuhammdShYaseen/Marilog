

namespace Marilog.Application.DTOs.Commands.Document
{
    public record AddDocumentItemCommand(
     string ProductName,
     decimal Quantity,
     decimal UnitPrice,
     string? Unit = null
 );
}
