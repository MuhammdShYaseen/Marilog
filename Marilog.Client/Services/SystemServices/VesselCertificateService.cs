using Marilog.Contracts.Common;
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
    }
}