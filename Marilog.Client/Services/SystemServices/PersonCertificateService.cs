using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.PersonDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.DTOs.Responses.Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;

namespace Marilog.Client.Services.SystemServices
{
    public class PersonCertificateService : IPersonCertificateService
    {
        private readonly HttpClient _http;
        private const string Base = "api/person-certificates";

        public PersonCertificateService(HttpClient http) => _http = http;

        public async Task<CertificateSummaryResponse> GetSummaryAsync(int? personId = null,
            bool includeInactivePersons = false, CancellationToken ct = default)
        {
            var url = $"{Base}/summary?includeInactivePersons={includeInactivePersons.ToString().ToLowerInvariant()}";
            if (personId is not null) url += $"&personId={personId}";

            var response = await _http.GetCheckedAsync<ApiResponse<CertificateSummaryResponse>>(url, ct);
            return response?.Data ?? new();
        }

        public async Task<PagedResponse<CertificateResponse>> GetPagedAsync(PagedRequest paging, PersonCertificateFilter filter, CancellationToken ct = default)
        {
            var query = new List<string>
            {
                $"page={paging.Page}",
                $"pageSize={paging.PageSize}",
                $"includeInactivePersons={filter.IncludeInactivePersons.ToString().ToLowerInvariant()}"
            };

            if (!string.IsNullOrWhiteSpace(paging.SearchTerm)) query.Add($"searchTerm={Uri.EscapeDataString(paging.SearchTerm)}");
            if (filter.PersonId is not null) query.Add($"personId={filter.PersonId}");
            if (filter.Status is not null) query.Add($"status={filter.Status}");
            if (filter.Type is not null) query.Add($"type={filter.Type}");

            var response = await _http.GetCheckedAsync<ApiResponse<PagedResponse<CertificateResponse>>>(
                $"{Base}?{string.Join("&", query)}", ct);

            return response?.Data ?? new PagedResponse<CertificateResponse> { Page = paging.Page, PageSize = paging.PageSize };
        }
    }
}