using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.BillOfLadingDTOs
{
    public record ChangeBlNumberRequest(string NewBlNumber);

    public record ChangeIssuanceTypeRequest(BlIssuanceType NewIssuanceType, int? MasterBlId = null);

    public record LinkToMasterBlRequest(int MasterBlId);
}
