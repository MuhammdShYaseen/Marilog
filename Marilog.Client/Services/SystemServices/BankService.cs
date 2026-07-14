using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.BankDTOs;
using Marilog.Contracts.DTOs.Requests.ContactsRequestDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Kernel.Primitives;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class BankService : IBankService
    {
        private const string BaseRoute = "api/banks";
        private readonly HttpClient _http;

        public BankService(HttpClient http) => _http = http;

        public async Task<BankResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _http.GetFromJsonAsync<BankResponse>($"{BaseRoute}/{id}", cancellationToken);

        public async Task<PagedResponse<BankResponse>> GetPagedAsync(
     int page = 1, int pageSize = 20, bool treeMode = false,
     CancellationToken cancellationToken = default)
     => await _http.GetFromJsonAsync<PagedResponse<BankResponse>>(
            $"{BaseRoute}?page={page}&pageSize={pageSize}&treeMode={treeMode}", cancellationToken)
        ?? new PagedResponse<BankResponse>();

        public async Task<IReadOnlyList<BankResponse>> GetAllActiveAsync(
            bool treeMode = false, CancellationToken cancellationToken = default)
            => await _http.GetFromJsonAsync<List<BankResponse>>(
                   $"{BaseRoute}/active?treeMode={treeMode}", cancellationToken)
               ?? new List<BankResponse>();

        public async Task<IReadOnlyList<BankResponse>> GetByCountryIdAsync(
            int countryId, bool treeMode = false, CancellationToken cancellationToken = default)
            => await _http.GetFromJsonAsync<List<BankResponse>>(
                   $"{BaseRoute}/by-country/{countryId}?treeMode={treeMode}", cancellationToken)
               ?? new List<BankResponse>();

        public async Task<List<LookupItem<int>>> GetLookupAsync(CancellationToken cancellationToken = default)
            => await _http.GetFromJsonAsync<List<LookupItem<int>>>(
                   $"{BaseRoute}/lookup", cancellationToken)
               ?? new List<LookupItem<int>>();

        public async Task<List<LookupItem<int>>> GetParentBanksLookupAsync(
            int? excludeBankId = null, CancellationToken cancellationToken = default)
        {
            var url = excludeBankId.HasValue
                ? $"{BaseRoute}/lookup/parents?excludeBankId={excludeBankId.Value}"
                : $"{BaseRoute}/lookup/parents";

            return await _http.GetFromJsonAsync<List<LookupItem<int>>>(url, cancellationToken)
                   ?? new List<LookupItem<int>>();
        }

        public async Task<BankResponse> CreateAsync(CreateBankRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync(BaseRoute, request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<BankResponse>(cancellationToken))!;
        }
        public async Task<IReadOnlyList<BankResponse>> CreateRangAsync(
            List<CreateBankRequest> requests, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync($"{BaseRoute}/batch", requests, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<BankResponse>>(cancellationToken)
                   ?? new List<BankResponse>();
        }
        public async Task<Result> UpdateAsync(UpdateBankRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"{BaseRoute}/{request.Id}", request, cancellationToken);
            return await ToResultAsync(response, cancellationToken);
        }

        public async Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsync($"{BaseRoute}/{id}/deactivate", null, cancellationToken);
            return await ToResultAsync(response, cancellationToken);
        }

        public async Task<Result> ActivateAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsync($"{BaseRoute}/{id}/activate", null, cancellationToken);
            return await ToResultAsync(response, cancellationToken);
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"{BaseRoute}/{id}", cancellationToken);
            return await ToResultAsync(response, cancellationToken);
        }

        public async Task<Result> AddEmailAsync(int bankId, AddEmailRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync($"{BaseRoute}/{bankId}/emails", request, cancellationToken);
            return await ToResultAsync(response, cancellationToken);
        }

        public async Task<Result> RemoveEmailAsync(int bankId, string email, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync(
                $"{BaseRoute}/{bankId}/emails/{Uri.EscapeDataString(email)}", cancellationToken);
            return await ToResultAsync(response, cancellationToken);
        }

        public async Task<Result> AddPhoneAsync(int bankId, AddPhoneRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync($"{BaseRoute}/{bankId}/phones", request, cancellationToken);
            return await ToResultAsync(response, cancellationToken);
        }

        public async Task<Result> RemovePhoneAsync(int bankId, string phoneNumber, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync(
                $"{BaseRoute}/{bankId}/phones/{Uri.EscapeDataString(phoneNumber)}", cancellationToken);
            return await ToResultAsync(response, cancellationToken);
        }

        private static async Task<Result> ToResultAsync(HttpResponseMessage response, CancellationToken ct)
        {
            if (response.IsSuccessStatusCode) return Result.Ok();
            var error = await response.Content.ReadAsStringAsync(ct);
            return Result.Fail(string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase ?? "Request failed." : error);
        }
    }
}