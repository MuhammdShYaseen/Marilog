namespace Marilog.Contracts.DTOs.Requests.DocumentDTOs
{
    public class AddDocumentItemRequest
    {
        public string ProductName { get; set; } = default!;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Unit { get; set; }
    }
}
