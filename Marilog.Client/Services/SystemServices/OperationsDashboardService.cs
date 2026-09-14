using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.DocumentReports;
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

        public async Task<IReadOnlyList<FinancialChartPointResponse>> GetFinancialSummaryAsync(DocumentFilterOptions options, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<FinancialChartPointResponse>>>(
                $"api/operations/dashboard/financial-summary{BuildQuery(options)}", ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<VoyageChartPointResponse>> GetVoyageSummaryAsync(DateOnly from, DateOnly to, int? vesselId = null, CancellationToken ct = default)
        {
            var query = $"?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}" + (vesselId.HasValue ? $"&vesselId={vesselId}" : "");
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<VoyageChartPointResponse>>>(
                $"api/operations/dashboard/voyage-summary{query}", ct);
            return response!.Data!;
        }

        private static string BuildQuery(DocumentFilterOptions o)
        {
            var parts = new List<string>();
            if (o.SupplierId.HasValue) parts.Add($"SupplierId={o.SupplierId}");
            if (o.BuyerId.HasValue) parts.Add($"BuyerId={o.BuyerId}");
            if (o.VesselId.HasValue) parts.Add($"VesselId={o.VesselId}");
            if (o.DocTypeId.HasValue) parts.Add($"DocTypeId={o.DocTypeId}");
            if (o.UnpaidOnly) parts.Add("UnpaidOnly=true");
            if (o.FromDate.HasValue) parts.Add($"FromDate={o.FromDate:yyyy-MM-dd}");
            if (o.ToDate.HasValue) parts.Add($"ToDate={o.ToDate:yyyy-MM-dd}");
            if (o.Side.HasValue) parts.Add($"Side={o.Side}");
            if (o.VoyageId.HasValue) parts.Add($"VoyageId={o.VoyageId}");
            return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
        }
    }
}