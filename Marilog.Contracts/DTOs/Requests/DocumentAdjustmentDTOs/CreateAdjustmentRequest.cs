

namespace Marilog.Contracts.DTOs.Requests.DocumentAdjustmentDTOs
{
    public class CreateAdjustmentRequest
    {
        public int DocumentId { get; set; }
        public decimal Amount { get; set; }
        public DateOnly AdjustmentDate { get; set; }
        public string? Reason { get; set; }
    }
}
