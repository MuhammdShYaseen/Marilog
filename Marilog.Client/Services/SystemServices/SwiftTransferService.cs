using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.SwiftTransferReports;
using Marilog.Contracts.DTOs.Requests.SwiftTransferDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class SwiftTransferService : ISwiftTransferService
    {
        private readonly HttpClient _http;
        private const string Base = "api/swifttransfers";

        public SwiftTransferService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<SwiftTransferResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<SwiftTransferResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<SwiftTransferResponse?> GetByReferenceAsync(string reference, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<SwiftTransferResponse>>($"{Base}/by-reference/{Uri.EscapeDataString(reference)}", ct);
            return response?.Data;
        }

        // No backend endpoint — flag when added
        public Task<SwiftTransferResponse?> GetWithPaymentsAsync(int id, CancellationToken ct = default)
            => throw new NotImplementedException("Endpoint not yet defined on the backend.");

        public async Task<IReadOnlyList<SwiftTransferResponse>> GetBySenderAsync(int companyId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<SwiftTransferResponse>>>($"{Base}/by-sender/{companyId}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<SwiftTransferResponse>> GetByReceiverAsync(int companyId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<SwiftTransferResponse>>>($"{Base}/by-receiver/{companyId}", ct);
            return response?.Data ?? [];
        }
        public async Task<IReadOnlyList<SwiftTransferResponse>> GetBySenderAndReceverAsync(int senderId, int receiverId, CancellationToken ct = default)
        {
            var response =
                await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<SwiftTransferResponse>>>(
                    $"{Base}/by-sender-receiver?senderId={senderId}&receiverId={receiverId}", ct);

            return response?.Data ?? [];
        }
        public async Task<IReadOnlyList<SwiftTransferResponse>> GetUnallocatedAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<SwiftTransferResponse>>>($"{Base}/unallocated", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<SwiftTransferResponse>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<SwiftTransferResponse>>>($"{Base}/by-date-range?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}", ct);
            return response?.Data ?? [];
        }

        public Task<SwiftTransferReport> GetSwiftTransfersReportAsync(SwiftTransferFilterOptions options, CancellationToken ct = default)
            => throw new NotImplementedException("Endpoint not yet defined on the backend.");

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<SwiftTransferResponse> CreateAsync(string swiftReference, DateOnly transactionDate,
            int currencyId, decimal amount, int? senderCompanyId = null, int? receiverCompanyId = null,
            int? senderBankId = null, int? receiverBankId = null,
            string? paymentReference = null, string? rawMessage = null,
            CancellationToken ct = default)
        {
            var request = new CreateSwiftTransferRequest
            {
                SwiftReference = swiftReference,
                TransactionDate = transactionDate,
                CurrencyId = currencyId,
                Amount = amount,
                SenderCompanyId = senderCompanyId,
                ReceiverCompanyId = receiverCompanyId,
                SenderBankId = senderBankId,
                ReceiverBankId = receiverBankId,
                PaymentReference = paymentReference,
                RawMessage = rawMessage
            };

            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<SwiftTransferResponse>>(ct);
            return response!.Data ?? new SwiftTransferResponse();
        }

        public async Task<IReadOnlyList<SwiftTransferResponse>> CreateRangeAsync(IEnumerable<CreateSwiftTransferRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", commands, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<SwiftTransferResponse>>>(ct);
            return response?.Data ?? [];
        }

        public async Task UpdateAsync(int id, int currencyId, decimal amount,
            int? senderBankId, int? receiverBankId,
            string? paymentReference, string? rawMessage,
            CancellationToken ct = default)
        {
            var request = new UpdateSwiftTransferRequest
            {
                CurrencyId = currencyId,
                Amount = amount,
                SenderBankId = senderBankId,
                ReceiverBankId = receiverBankId,
                PaymentReference = paymentReference,
                RawMessage = rawMessage
            };

            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PostAsync($"{Base}/{id}/activate", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PostAsync($"{Base}/{id}/deactivate", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{id}", ct);
            http.EnsureSuccessStatusCode();
        }
    }
}