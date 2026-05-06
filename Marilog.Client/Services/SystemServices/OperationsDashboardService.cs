using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class OperationsDashboardService : IOperationsDashboardService
    {
        private readonly HttpClient _http;

        public OperationsDashboardService(HttpClient http) => _http = http;

        public async Task<OperationsDashboardResponse> GetAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<OperationsDashboardResponse>>("api/operations/dashboard", ct);
            return response!.Data!;
        }
    }
}