
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.DTOs.Requests.DocumentAdjustmentDTOs;
using Marilog.Contracts.DTOs.Requests.DocumentDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Globalization;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class DocumentService : IDocumentService
    {
        private readonly HttpClient _http;
        private const string Base = "api/documents";

        public DocumentService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<DocumentResponse?> GetByIdAsync(int id, bool treeView = false, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<DocumentResponse>>($"{Base}/{id}?treeView={treeView}", ct);
            return response?.Data;
        }

        public async Task<DocumentResponse?> GetWithItemsAsync(int id, bool treeView = false, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<DocumentResponse>>($"{Base}/{id}/with-items?treeView={treeView}", ct);
            return response?.Data;
        }

        public async Task<DocumentResponse?> GetWithPaymentsAsync(int id, bool treeView = false, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<DocumentResponse>>($"{Base}/{id}/with-payments?treeView={treeView}", ct);
            return response?.Data;
        }

        public async Task<DocumentResponse?> GetFullAsync(int id, bool treeView = false, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<DocumentResponse>>($"{Base}/{id}/full?treeView={treeView}", ct);
            return response?.Data;
        }

        public async Task<DocumentResponse?> GetByNumberAsync(string docNumber, bool treeView = false, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<DocumentResponse>>($"{Base}/by-number/{Uri.EscapeDataString(docNumber)}?treeView={treeView}", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetBySupplierAsync(int supplierId, bool treeView = false, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>($"{Base}/by-supplier/{supplierId}?treeView={treeView}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<DocumentResponse>> SearchAsync(string term, bool treeView = false, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>($"{Base}/vector-search/{term}?treeView={treeView}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetByBuyerAsync(int buyerId, bool treeView = false, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>($"{Base}/by-buyer/{buyerId}?treeView={treeView}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetByVesselAsync(int vesselId, bool treeView = false, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>($"{Base}/by-vessel/{vesselId}?treeView={treeView}", ct);
            return response?.Data ?? [];
        }
        public async Task<IReadOnlyList<DocumentResponse>> GetLastAddedAsync(int days, bool treeView = false, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>($"{Base}/last-added/{days}?treeView={treeView}", ct);
            return response?.Data ?? [];
        }
        public async Task<IReadOnlyList<DocumentResponse>> GetByVoyageAsync(int voyageId, bool treeView = false, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>($"{Base}/by-voyage/{voyageId}?treeView={treeView}", ct);
            return response?.Data ?? [];
        }
        public async Task<IReadOnlyList<DocumentResponse>> GetByTypeAsync(int docTypeId, bool treeView = false, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>($"{Base}/by-type/{docTypeId}?treeView={treeView}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetUnpaidAsync(bool treeView = false, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>($"{Base}/unpaid?treeView={treeView}", ct);
            return response?.Data ?? [];
        }

        public async Task<PagedResponse<DocumentResponse>> GetUnpaidPagedAsync(PagedRequest request, bool treeView = false, CancellationToken ct = default)
        {
            var url = $"{Base}/unpaid/paged?page={request.Page}&pageSize={request.PageSize}&treeView={treeView}";

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                url += $"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}";

            return await _http.GetCheckedAsync<PagedResponse<DocumentResponse>>(url, ct) ?? new PagedResponse<DocumentResponse>
            {
                 Page = request.Page,
                 PageSize = request.PageSize,
                 TotalPages = 0
            };
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetChildrenAsync(int parentDocumentId, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>($"{Base}/{parentDocumentId}/children", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetAllAsTreeAsync(CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>(
                $"{Base}/tree", ct);
            return response?.Data ?? [];
        }

        public async Task<DocumentResponse?> GetTreeByDocumentIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<DocumentResponse>>(
                $"{Base}/{id}/tree", ct);
            return response?.Data;
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<DocumentResponse> CreateAsync(CreateDocumentRequest request, CancellationToken ct = default)
        {
            

            var http = await _http.PostAsJsonAsync(Base, request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<DocumentResponse>>(ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<DocumentResponse>> CreateRangeAsync(IEnumerable<CreateDocumentRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", commands, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>(ct);
            return response?.Data ?? [];
        }

        public async Task UpdateAsync(int id, UpdateDocumentRequest request, CancellationToken ct = default)
        {
            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task LinkToParentAsync(int id, int parentDocumentId, CancellationToken ct = default)
        {
            var request = new LinkParentRequest { ParentDocumentId = parentDocumentId };
            var http = await _http.PatchAsJsonAsync($"{Base}/{id}/link-parent", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task UnlinkFromParentAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/unlink-parent", null, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/activate", null, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/deactivate", null, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{id}", ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        // ── Items ─────────────────────────────────────────────────────────────────

        public async Task<DocumentItemResponse> AddItemAsync(int documentId, string productName,
            decimal quantity, decimal unitPrice, string? unit = null, CancellationToken ct = default)
        {
            var request = new AddDocumentItemRequest { ProductName = productName, Quantity = quantity, UnitPrice = unitPrice, Unit = unit };
            var http = await _http.PostAsJsonAsync($"{Base}/{documentId}/items", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<DocumentItemResponse>(ct);
            return response!;
        }

        public async Task<IReadOnlyList<DocumentItemResponse>> AddItemsRangeAsync(int documentId, IEnumerable<AddDocumentItemRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/{documentId}/items/batch", commands, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<DocumentItemResponse>>>(ct);
            return response?.Data ?? [];
        }

        public async Task UpdateItemAsync(int documentId, int itemId, string productName,
            decimal quantity, decimal unitPrice, string? unit = null, CancellationToken ct = default)
        {
            var request = new UpdateDocumentItemRequest { ProductName = productName, Quantity = quantity, UnitPrice = unitPrice, Unit = unit };
            var http = await _http.PutAsJsonAsync($"{Base}/{documentId}/items/{itemId}", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task RemoveItemAsync(int documentId, int itemId, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{documentId}/items/{itemId}", ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }



        public async Task<IReadOnlyList<PriceHistoryResponse>> GetPriceHistoryAsync(string productName, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string>{ $"productName={Uri.EscapeDataString(productName)}"};

            if (from.HasValue)
                query.Add($"from={from.Value:yyyy-MM-dd}");

            if (to.HasValue)
                query.Add($"to={to.Value:yyyy-MM-dd}");

            var url = $"{Base}/item-price-history?{string.Join("&", query)}";

            return await _http.GetCheckedAsync<IReadOnlyList<PriceHistoryResponse>>(url,  cancellationToken) ?? [];
        }

        // -- Adjustment ------------------------------------------------------------
        public async Task<AdjustmentResponse> AddAdjustmentAsync(CreateAdjustmentRequest request, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/adjustments", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<AdjustmentResponse>(ct);
            return response!;
        }

        public async Task<AdjustmentResponse> UpdateAdjustmentAsync(int adjustmentId, UpdateAdjustmentRequest request, CancellationToken ct = default)
        {
            var http = await _http.PutAsJsonAsync($"{Base}/adjustments/{adjustmentId}", request, ct);

            await http.EnsureSuccessWithMessageAsync(ct);

            var response = await http.Content.ReadFromJsonAsync<AdjustmentResponse>(ct);

            return response!;

        }

        public async Task RemoveAdjustmentAsync(int adjustmentId, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/adjustment/{adjustmentId}", ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        //--------Report--------------------------------------------------------------------------------
        public async Task<DocumentReport> GetFilteredDocsReportAsync(DocumentFilterOptions options, CancellationToken ct = default)
        {
            var query = BuildDocumentQueryString(options);
            return await _http.GetApiAsync<DocumentReport>($"api/reports/documents{query}", ct) ?? new DocumentReport();
        }

        private static string BuildDocumentQueryString(DocumentFilterOptions options)
        {
            var parts = new List<string>();

            if (options.SupplierId.HasValue)
                parts.Add($"supplierId={options.SupplierId.Value}");

            if (options.BuyerId.HasValue)
                parts.Add($"buyerId={options.BuyerId.Value}");

            if (options.VesselId.HasValue)
                parts.Add($"vesselId={options.VesselId.Value}");

            if (options.DocTypeId.HasValue)
                parts.Add($"docTypeId={options.DocTypeId.Value}");

            if (options.UnpaidOnly)
                parts.Add("unpaidOnly=true");

            if (options.LastDays.HasValue)
                parts.Add($"lastDays={options.LastDays.Value}");

            if (options.Year.HasValue)
                parts.Add($"year={options.Year.Value}");

            if (options.Month.HasValue)
                parts.Add($"month={options.Month.Value}");

            if (options.FromDate.HasValue)
                parts.Add($"fromDate={options.FromDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

            if (options.ToDate.HasValue)
                parts.Add($"toDate={options.ToDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

            if (options.Side.HasValue)
                parts.Add($"side={options.Side.Value}");

            return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
        }
       
    }
}