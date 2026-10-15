using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.VesselDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.DTOs.Responses.Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class VesselCertificateService : IVesselCertificateService
    {
        private readonly HttpClient _http;
        private const string Base = "api/vessel-certificates";

        public VesselCertificateService(HttpClient http) => _http = http;

        public async Task<CertificateSummaryResponse> GetSummaryAsync(int? vesselId = null,
            bool includeInactiveVessels = false, CancellationToken ct = default)
        {
            var url = $"{Base}/summary?includeInactiveVessels={includeInactiveVessels.ToString().ToLowerInvariant()}";
            if (vesselId is not null) url += $"&vesselId={vesselId}";

            var response = await _http.GetFromJsonAsync<ApiResponse<CertificateSummaryResponse>>(url, ct);
            return response?.Data ?? new();
        }

        public async Task<PagedResponse<CertificateResponse>> GetPagedAsync(PagedRequest paging, VesselCertificateFilter filter, CancellationToken ct = default)
        {
            var query = new List<string>
            {
                $"page={paging.Page}",
                $"pageSize={paging.PageSize}",
                $"includeInactiveVessels={filter.IncludeInactiveVessels.ToString().ToLowerInvariant()}"
            };

            if (!string.IsNullOrWhiteSpace(paging.SearchTerm)) query.Add($"searchTerm={Uri.EscapeDataString(paging.SearchTerm)}");
            if (filter.VesselId is not null) query.Add($"vesselId={filter.VesselId}");
            if (filter.Status is not null) query.Add($"status={filter.Status}");
            if (filter.Type is not null) query.Add($"type={filter.Type}");

            var response = await _http.GetFromJsonAsync<ApiResponse<PagedResponse<CertificateResponse>>>(
                $"{Base}?{string.Join("&", query)}", ct);

            return response?.Data ?? new PagedResponse<CertificateResponse> { Page = paging.Page, PageSize = paging.PageSize };
        }
    }
}