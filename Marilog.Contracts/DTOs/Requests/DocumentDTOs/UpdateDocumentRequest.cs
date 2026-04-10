namespace Marilog.Contracts.DTOs.Requests.DocumentDTOs
{
    public class UpdateDocumentRequest
    {
        public int DocTypeId { get; set; }
        public DateOnly DocDate { get; set; }
        public int CurrencyId { get; set; }
        public decimal TotalAmount { get; set; }
        public int? SupplierId { get; set; }
        public int? BuyerId { get; set; }
        public int? VesselId { get; set; }
        public int? PortId { get; set; }
        public string? Reference { get; set; }
        public string? FilePath { get; set; }
    }
}
