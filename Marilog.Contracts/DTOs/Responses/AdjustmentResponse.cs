

namespace Marilog.Contracts.DTOs.Responses
{
    public class AdjustmentResponse
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public decimal Amount { get; set; }
        public DateOnly AdjustmentDate { get; set; }
        public string? Reason { get; set; }
    }
}
