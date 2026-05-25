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

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<DocumentResponse> CreateAsync(string docNumber, int docTypeId, DateOnly docDate,
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

        public async Task UpdateAsync(int id, int docTypeId, DateOnly docDate,
            int currencyId, decimal totalAmount, int? supplierId = null, int? buyerId = null,
            int? vesselId = null, int? portId = null, int? parentDocumentId = null, string? reference = null
            , CancellationToken ct = default)
        {
            var request = new UpdateDocumentRequest
            {
                DocTypeId = docTypeId,
                DocDate = docDate,
                CurrencyId = currencyId,
                TotalAmount = totalAmount,
                SupplierId = supplierId,
                BuyerId = buyerId,
                VesselId = vesselId,
                PortId = portId,
                Reference = reference,
                ParentDocumentId = parentDocumentId,
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

        // ── Emails ────────────────────────────────────────────────────────────────

        public async Task LogEmailAsync(int documentId, string subject, string body,
            IReadOnlyList<EmailParticipantResponse> participants,
            EmailDirection direction = EmailDirection.Outbound, CancellationToken ct = default)
        {
            var request = new LogEmailRequest { Subject = subject, Body = body, Participants = participants, Direction = direction };
            var http = await _http.PostAsJsonAsync($"{Base}/{documentId}/email", request, ct);
            http.EnsureSuccessStatusCode();
        }

        // ── Reports ───────────────────────────────────────────────────────────────

        public Task<DocumentReport> GetFilteredDocsReportAsync(DocumentFilterOptions options, CancellationToken ct = default)
            => throw new NotImplementedException("Endpoint not yet defined on the backend.");


    }
}