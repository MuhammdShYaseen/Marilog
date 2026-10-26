using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.SwiftTransferDTOs
{
    public class UpdateSwiftChargeDetailsRequest
    {
        public SwiftChargeBearer ChargeBearer { get; set; }
        public decimal? InstructedAmount { get; set; }
    }
}