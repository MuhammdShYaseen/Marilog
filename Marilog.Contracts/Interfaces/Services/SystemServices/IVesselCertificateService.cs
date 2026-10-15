using Marilog.Contracts.DTOs.Responses.Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IVesselCertificateService
    {
        Task<CertificateSummaryResponse> GetSummaryAsync(int? vesselId = null, bool includeInactiveVessels = false, CancellationToken ct = default);
    }
}
