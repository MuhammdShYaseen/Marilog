using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.VesselDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.DTOs.Responses.Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IVesselCertificateService
    {
        Task<CertificateSummaryResponse> GetSummaryAsync(int? vesselId = null, bool includeInactiveVessels = false, CancellationToken ct = default);
        Task<PagedResponse<CertificateResponse>> GetPagedAsync(PagedRequest paging, VesselCertificateFilter filter, CancellationToken ct = default);
    }
}
