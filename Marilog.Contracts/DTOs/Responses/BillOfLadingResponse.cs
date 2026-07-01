

using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{

    public class BillOfLadingResponse
    {
        public int Id { get; init; }
        public int VoyageId { get; init; }
        public string? VoyageNumber { get; init; }
        public string? BlNumber { get; init; }
        public string? BlType { get; init; }
        public BlIssuanceType IssuanceType { get; init; }
        public CompanyResponse? ShipperCompany { get; init; }
        public CompanyResponse? ConsigneeCompany { get; init; }
        public string? ConsigneeToOrder { get; init; }
        public CompanyResponse? NotifyPartyCompany { get; init; }
        public CompanyResponse? CarrierCompany { get; init; }
        public PortResponse? PortOfLoading { get; init; }
        public PortResponse? PortOfDischarge { get; init; }
        public PortResponse? PlaceOfReceipt { get; init; }
        public PortResponse? PlaceOfDelivery { get; init; }
        public string? CargoDescription { get; init; }
        public string? HsCode { get; init; }
        public decimal? GrossWeightMT { get; init; }
        public decimal? VolumeM3 { get; init; }
        public int? PackageCount { get; init; }
        public string? PackageType { get; init; }
        public string? MarksAndNumbers { get; init; }
        public string? FreightTerms { get; init; }
        public string? FreightAmount { get; init; }
        public string? Incoterms { get; init; }
        public DateOnly? IssueDate { get; init; }
        public string? PlaceOfIssue { get; init; }
        public DateOnly? OnBoardDate { get; init; }
        public int? OriginalCopiesCount { get; init; }
        public int? MasterBlId { get; init; }
        public string? MasterBlNumber { get; init; }
        public string? Notes {  get; init; }
        public DateTime? CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}
