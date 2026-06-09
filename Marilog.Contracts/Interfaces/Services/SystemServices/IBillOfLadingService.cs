using Marilog.Contracts.DTOs.Requests.BillOfLadingDTOs;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IBillOfLadingService
    {
        Task<BillOfLadingResponse> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<BillOfLadingResponse>> GetByVoyageAsync(int voyageId, CancellationToken ct = default);
        Task<BillOfLadingResponse> CreateAsync(CreateBillOfLadingRequest request, CancellationToken ct = default);
        Task<BillOfLadingResponse> UpdateAsync(int id, UpdateBillOfLadingRequest request, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);

        // الحساسة — endpoints منفصلة
        Task<BillOfLadingResponse> ChangeBlNumberAsync(int id, ChangeBlNumberRequest request, CancellationToken ct = default);
        Task<BillOfLadingResponse> ChangeIssuanceTypeAsync(int id, ChangeIssuanceTypeRequest request, CancellationToken ct = default);
        Task<BillOfLadingResponse> LinkToMasterBlAsync(int id, LinkToMasterBlRequest request, CancellationToken ct = default);
    }
}
