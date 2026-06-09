using Marilog.Kernel.Enums;


namespace Marilog.Contracts.DTOs.Requests.BillOfLadingDTOs
{
    public record CreateBillOfLadingRequest(
    int VoyageId,
    string BlNumber,
    BlType BlType,
    BlIssuanceType IssuanceType,
    int ShipperCompanyId,
    int CarrierCompanyId,
    int PortOfLoadingId,
    int PortOfDischargeId,
    string CargoDescription,
    decimal GrossWeightMT,
    FreightTerms FreightTerms,
    DateOnly IssueDate,
    int? ConsigneeCompanyId = null,
    string? ConsigneeToOrder = null,
    int? NotifyPartyCompanyId = null,
    string? HsCode = null,
    decimal? VolumeM3 = null,
    int? PackageCount = null,
    string? PackageType = null,
    string? MarksAndNumbers = null,
    string? FreightAmount = null,
    string? Incoterms = null,
    string? PlaceOfIssue = null,
    DateOnly? OnBoardDate = null,
    int OriginalCopiesCount = 3,
    int? MasterBlId = null,
    int? PlaceOfReceiptPortId = null,
    int? PlaceOfDeliveryPortId = null,
    string? Notes = null
);
}
