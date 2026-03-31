

namespace Marilog.Application.DTOs.Commands.Document
{
    public record CreateDocumentCommand(
     string DocNumber,
     int DocTypeId,
     DateOnly DocDate,
     int CurrencyId,
     decimal TotalAmount,
     int? SupplierId = null,
     int? BuyerId = null,
     int? VesselId = null,
     int? PortId = null,
     int? ParentDocumentId = null,
     string? Reference = null,
     string? FilePath = null
 );
}
