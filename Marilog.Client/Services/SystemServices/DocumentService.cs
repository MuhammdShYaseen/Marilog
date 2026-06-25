using Marilog.Client.Extensions;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.DTOs.Requests.DocumentDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Kernel.Enums;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class DocumentService : IDocumentService
    {
        private readonly HttpClient _http;
        private const string Base = "api/documents";

        public DocumentService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<DocumentResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<DocumentResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<DocumentResponse?> GetWithItemsAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<DocumentResponse>>($"{Base}/{id}/with-items", ct);
            return response?.Data;
        }

        public async Task<DocumentResponse?> GetWithPaymentsAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<DocumentResponse>>($"{Base}/{id}/with-payments", ct);
            return response?.Data;
        }

        public async Task<DocumentResponse?> GetFullAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<DocumentResponse>>($"{Base}/{id}/full", ct);
            return response?.Data;
        }

        public async Task<DocumentResponse?> GetByNumberAsync(string docNumber, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<DocumentResponse>>($"{Base}/by-number/{Uri.EscapeDataString(docNumber)}", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetBySupplierAsync(int supplierId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>($"{Base}/by-supplier/{supplierId}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<DocumentResponse>> SearchAsync(string term, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>($"{Base}/vector-search/{term}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetByBuyerAsync(int buyerId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>($"{Base}/by-buyer/{buyerId}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetByVesselAsync(int vesselId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>($"{Base}/by-vessel/{vesselId}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetByTypeAsync(int docTypeId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>($"{Base}/by-type/{docTypeId}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetUnpaidAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>($"{Base}/unpaid", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetChildrenAsync(int parentDocumentId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>($"{Base}/{parentDocumentId}/children", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetAllAsTreeAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>(
                $"{Base}/tree", ct);
            return response?.Data ?? [];
        }

        public async Task<DocumentResponse?> GetTreeByDocumentIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<DocumentResponse>>(
                $"{Base}/{id}/tree", ct);
            return response?.Data;
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<DocumentResponse> CreateAsync(string docNumber, int docTypeId, FinancialSide side, DateOnly docDate,
            int currencyId, decimal totalAmount, int? supplierId = null, int? buyerId = null,
            int? vesselId = null, int? portId = null, int? parentDocumentId = null,
            string? reference = null, CancellationToken ct = default)
        {
            var request = new CreateDocumentRequest
            {
                DocNumber = docNumber,
                DocTypeId = docTypeId,
                DocDate = docDate,
                CurrencyId = currencyId,
                TotalAmount = totalAmount,
                SupplierId = supplierId,
                BuyerId = buyerId,
                VesselId = vesselId,
                PortId = portId,
                ParentDocumentId = parentDocumentId,
                Reference = reference,
                Side = side
            };

            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<DocumentResponse>>(ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<DocumentResponse>> CreateRangeAsync(IEnumerable<CreateDocumentRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", commands, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<DocumentResponse>>>(ct);
            return response?.Data ?? [];
        }

        public async Task UpdateAsync(int id,string docNumber, int docTypeId, FinancialSide side, DateOnly docDate,
            int currencyId, decimal totalAmount, int? supplierId = null, int? buyerId = null,
            int? vesselId = null, int? portId = null, int? parentDocumentId = null, string? reference = null
            , CancellationToken ct = default)
        {
            var request = new UpdateDocumentRequest
            {
                DocTypeId = docTypeId,
                DocNumber = docNumber,
                DocDate = docDate,
                CurrencyId = currencyId,
                TotalAmount = totalAmount,
                SupplierId = supplierId,
                BuyerId = buyerId,
                VesselId = vesselId,
                PortId = portId,
                Reference = reference,
                ParentDocumentId = parentDocumentId,
                Side = side,
            };

            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task LinkToParentAsync(int id, int parentDocumentId, CancellationToken ct = default)
        {
            var request = new LinkParentRequest { ParentDocumentId = parentDocumentId };
            var http = await _http.PatchAsJsonAsync($"{Base}/{id}/link-parent", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task UnlinkFromParentAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/unlink-parent", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/activate", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/deactivate", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{id}", ct);
            http.EnsureSuccessStatusCode();
        }

        // ── Items ─────────────────────────────────────────────────────────────────

        public async Task<DocumentItemResponse> AddItemAsync(int documentId, string productName,
            decimal quantity, decimal unitPrice, string? unit = null, CancellationToken ct = default)
        {
            var request = new AddDocumentItemRequest { ProductName = productName, Quantity = quantity, UnitPrice = unitPrice, Unit = unit };
            var http = await _http.PostAsJsonAsync($"{Base}/{documentId}/items", request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<DocumentItemResponse>(ct);
            return response!;
        }

        public async Task<IReadOnlyList<DocumentItemResponse>> AddItemsRangeAsync(int documentId, IEnumerable<AddDocumentItemRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/{documentId}/items/batch", commands, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<DocumentItemResponse>>>(ct);
            return response?.Data ?? [];
        }

        public async Task UpdateItemAsync(int documentId, int itemId, string productName,
            decimal quantity, decimal unitPrice, string? unit = null, CancellationToken ct = default)
        {
            var request = new UpdateDocumentItemRequest { ProductName = productName, Quantity = quantity, UnitPrice = unitPrice, Unit = unit };
            var http = await _http.PutAsJsonAsync($"{Base}/{documentId}/items/{itemId}", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task RemoveItemAsync(int documentId, int itemId, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{documentId}/items/{itemId}", ct);
            http.EnsureSuccessStatusCode();
        }

        // ── Payments ──────────────────────────────────────────────────────────────

        public async Task<PaymentResponse> AddPaymentAsync(int documentId, int swiftTransferId,
            decimal paidAmount, DateOnly paymentDate, CancellationToken ct = default)
        {
            var request = new AddPaymentRequest { SwiftTransferId = swiftTransferId, PaidAmount = paidAmount, PaymentDate = paymentDate };
            var http = await _http.PostAsJsonAsync($"{Base}/{documentId}/payments", request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<PaymentResponse>(ct);
            return response!;
        }

        public async Task<PaymentResponse> UpdatePaymentAsync(int documentId, int paymentId, int swiftTransferId, decimal paidAmount, DateOnly paymentDate, CancellationToken ct = default)
        {
            var request = new UpdatePaymentRequest
            {
                SwiftTransferId = swiftTransferId,
                PaidAmount = paidAmount,
                PaymentDate = paymentDate
            };

            var http = await _http.PutAsJsonAsync( $"{Base}/{documentId}/payments/{paymentId}",  request, ct);

            http.EnsureSuccessStatusCode();

            var response = await http.Content.ReadFromJsonAsync<PaymentResponse>(ct);

            return response!;
        }

        // ── Emails ────────────────────────────────────────────────────────────────

        public async Task LogEmailAsync(int documentId, string subject, string body,
            IReadOnlyList<EmailParticipantResponse> participants,
            EmailDirection direction = EmailDirection.Outbound, CancellationToken ct = default)
        {
            var request = new LogEmailRequest { Subject = subject, Body = body, Participants = participants, Direction = direction };
            var http = await _http.PostAsJsonAsync($"{Base}/{documentId}/email", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task<DocumentReport> GetFilteredDocsReportAsync(DocumentFilterOptions options, CancellationToken ct = default)
        {
            var query = BuildQueryString(options);
            return await _http.GetApiAsync<DocumentReport>($"api/reports/documents{query}", ct) ?? new DocumentReport();
        }

        private static string BuildQueryString(DocumentFilterOptions options)
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
                parts.Add($"fromDate={options.FromDate.Value:yyyy-MM-dd}");
            if (options.ToDate.HasValue)
                parts.Add($"toDate={options.ToDate.Value:yyyy-MM-dd}");
            if (options.Side.HasValue)
                parts.Add($"side={options.Side.Value}");
            return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
        }

        
    }
}