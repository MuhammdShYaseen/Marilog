using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.PersonDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.DTOs.Responses.Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IPersonCertificateService
    {
        Task<CertificateSummaryResponse> GetSummaryAsync(int? personId = null, bool includeInactivePersons = false, CancellationToken ct = default);
        Task<PagedResponse<CertificateResponse>> GetPagedAsync(PagedRequest paging, PersonCertificateFilter filter, CancellationToken ct = default);
        Task<IReadOnlyList<string>> GetNamesAsync(CancellationToken ct = default);
    }
}