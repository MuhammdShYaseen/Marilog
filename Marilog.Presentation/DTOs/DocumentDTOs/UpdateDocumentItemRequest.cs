namespace Marilog.Presentation.DTOs.DocumentDTOs
{
    public class UpdateDocumentItemRequest
    {
        public string ProductName { get; set; } = default!;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Unit { get; set; }
    }
}
