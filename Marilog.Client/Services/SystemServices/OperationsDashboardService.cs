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
            var response = await _http.GetCheckedAsync<ApiResponse<OperationsDashboardResponse>>("api/operations/dashboard", ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<FinancialChartPointResponse>> GetFinancialSummaryAsync(DocumentFilterOptions options, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<FinancialChartPointResponse>>>(
                $"api/operations/dashboard/financial-summary{BuildQuery(options)}", ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<VoyageChartPointResponse>> GetVoyageSummaryAsync(DateOnly from, DateOnly to, int? vesselId = null, CancellationToken ct = default)
        {
            var query = $"?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}" + (vesselId.HasValue ? $"&vesselId={vesselId}" : "");
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<VoyageChartPointResponse>>>(
                $"api/operations/dashboard/voyage-summary{query}", ct);
            return response!.Data!;
        }
        public async Task<IReadOnlyList<CurrencyTickerItemResponse>> GetCurrencyTickerAsync(CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<List<CurrencyTickerItemResponse>>(
                "api/operations/dashboard/currency-ticker", ct);
            return response ?? [];
        }
        private static string BuildQuery(DocumentFilterOptions o)
        {
            var parts = new List<string>();
            parts.AddRange(o.SupplierIds.Select(id => $"SupplierIds={id}"));
            parts.AddRange(o.BuyerIds.Select(id => $"BuyerIds={id}"));
            parts.AddRange(o.VesselIds.Select(id => $"VesselIds={id}"));
            parts.AddRange(o.DocTypeIds.Select(id => $"DocTypeIds={id}"));
            if (o.UnpaidOnly) parts.Add("UnpaidOnly=true");
            if (o.FromDate.HasValue) parts.Add($"FromDate={o.FromDate:yyyy-MM-dd}");
            if (o.ToDate.HasValue) parts.Add($"ToDate={o.ToDate:yyyy-MM-dd}");
            if (o.Side.HasValue) parts.Add($"Side={o.Side}");
            parts.AddRange(o.VoyageIds.Select(id => $"VoyageIds={id}"));
            return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
        }

        
    }
}