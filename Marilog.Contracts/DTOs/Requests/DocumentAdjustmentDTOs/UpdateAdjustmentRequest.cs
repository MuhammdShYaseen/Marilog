

namespace Marilog.Contracts.DTOs.Requests.DocumentAdjustmentDTOs
{
    public class UpdateAdjustmentRequest
    {
        public decimal Amount { get; set; }
        public DateOnly AdjustmentDate { get; set; }
        public string? Reason { get; set; }
    }
}
