

namespace Marilog.Contracts.DTOs.Responses
{
    
    public record BillOfLadingResponse(
        int Id,
        int VoyageId,
        string VoyageNumber,
        string BlNumber,
        string BlType,
        string IssuanceType,
        CompanyResponse ShipperCompany,
        CompanyResponse? ConsigneeCompany,
        string? ConsigneeToOrder,
        CompanyResponse? NotifyPartyCompany,
        CompanyResponse CarrierCompany,
        PortResponse PortOfLoading,
        PortResponse PortOfDischarge,
        PortResponse? PlaceOfReceipt,
        PortResponse? PlaceOfDelivery,
        string CargoDescription,
        string? HsCode,
        decimal GrossWeightMT,
        decimal? VolumeM3,
        int? PackageCount,
        string? PackageType,
        string? MarksAndNumbers,
        string FreightTerms,
        string? FreightAmount,
        string? Incoterms,
        DateOnly IssueDate,
        string? PlaceOfIssue,
        DateOnly? OnBoardDate,
        int OriginalCopiesCount,
        int? MasterBlId,
        string? MasterBlNumber,
        string? Notes,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
