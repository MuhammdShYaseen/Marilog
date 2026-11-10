using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Kernel.Enums;

namespace Marilog.Client.Services.SystemServices
{
    public class RecordLinkService : IRecordLinkService
    {
        private readonly HttpClient _http;
        private const string Base = "api/record-links";

        public RecordLinkService(HttpClient http) => _http = http;

        public async Task<RecordLinkResponse?> ResolveAsync(EntityType type, int id, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<RecordLinkResponse>>(
                $"{Base}/{type}/{id}", ct);

            return response?.Data;
        }
    }
}