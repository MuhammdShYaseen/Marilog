using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.PersonDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class PersonService : IPersonService
    {
        private readonly HttpClient _http;
        private const string Base = "api/person";

        public PersonService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<PersonResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<PersonResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<PersonResponse?> GetByPassportAsync(string passportNo, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<PersonResponse>>($"{Base}/passport/{Uri.EscapeDataString(passportNo)}", ct);
            return response?.Data;
        }

        public async Task<PersonResponse?> GetBySeamanBookAsync(string seamanBookNo, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<PersonResponse>>($"{Base}/seamanbook/{Uri.EscapeDataString(seamanBookNo)}", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<PersonResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<PersonResponse>>>(Base, ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<PersonResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<PersonResponse>>>($"{Base}/active", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<PersonResponse>> SearchAsync(string term, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<PersonResponse>>>($"{Base}/search?term={Uri.EscapeDataString(term)}", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<PersonResponse>> GetWithExpiringPassportsAsync(int withinDays, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<PersonResponse>>>($"{Base}/expiring-passports?withinDays={withinDays}", ct);
            return response?.Data ?? [];
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<PersonResponse> CreateAsync(string? bankName, string? iBAN, bool isPassportExpired,
            string? bankSwiftCode, string fullName, int? nationality = null,
            string? passportNo = null, DateOnly? passportExpiry = null,
            string? seamanBookNo = null, DateOnly? dateOfBirth = null,
            string? phone = null, string? email = null, CancellationToken ct = default)
        {
            var request = new CreatePersonRequest
            {
                BankName = bankName,
                IBAN = iBAN,
                IsPassportExpired = isPassportExpired,
                BankSwiftCode = bankSwiftCode,
                FullName = fullName,
                Nationality = nationality,
                PassportNo = passportNo,
                PassportExpiry = passportExpiry,
                SeamanBookNo = seamanBookNo,
                DateOfBirth = dateOfBirth,
                Phone = phone,
                Email = email
            };

            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<PersonResponse>>(ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<PersonResponse>> CreateRangeAsync(IEnumerable<CreatePersonRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", commands, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<PersonResponse>>>(ct);
            return response?.Data ?? [];
        }

        public async Task UpdateAsync(int id, string fullName, int? nationality = null,
            string? passportNo = null, DateOnly? passportExpiry = null,
            string? seamanBookNo = null, DateOnly? dateOfBirth = null,
            string? phone = null, string? email = null, CancellationToken ct = default)
        {
            var request = new UpdatePersonRequest
            {
                FullName = fullName,
                Nationality = nationality,
                PassportNo = passportNo,
                PassportExpiry = passportExpiry,
                SeamanBookNo = seamanBookNo,
                DateOfBirth = dateOfBirth,
                Phone = phone,
                Email = email
            };

            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
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

        // ── Bank Account ──────────────────────────────────────────────────────────

        public async Task UpdateBankAccountAsync(int id, string? bankName, string? iban,
            string? bankSwiftCode, CancellationToken ct = default)
        {
            var request = new UpdateBankAccountRequest 
            { 
                BankName = bankName,
                IBAN = iban,
                BankSwiftCode = bankSwiftCode
            };
            var http = await _http.PutAsJsonAsync($"{Base}/{id}/bank-account", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task ClearBankAccountAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PostAsync($"{Base}/{id}/bank-account/clear", null, ct);
            http.EnsureSuccessStatusCode();
        }


        // ── Certificates ─────────────────────────────────────────────────────────────

        public async Task AddCertificateAsync(int personId,
            UpsertCertificateRequest request, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync(
                $"{Base}/{personId}/certificates", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task UpdateCertificateAsync(int personId, int index,
            UpsertCertificateRequest request, CancellationToken ct = default)
        {
            var http = await _http.PutAsJsonAsync(
                $"{Base}/{personId}/certificates/{index}", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task RemoveCertificateAsync(int personId, int index,
            CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync(
                $"{Base}/{personId}/certificates/{index}", ct);
            http.EnsureSuccessStatusCode();
        }

        // ── Sea Services ─────────────────────────────────────────────────────────────

        public async Task AddSeaServiceAsync(int personId,
            UpsertSeaServiceRequest request, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync(
                $"{Base}/{personId}/sea-services", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task UpdateSeaServiceAsync(int personId, int index,
            UpsertSeaServiceRequest request, CancellationToken ct = default)
        {
            var http = await _http.PutAsJsonAsync(
                $"{Base}/{personId}/sea-services/{index}", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task RemoveSeaServiceAsync(int personId, int index,
            CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync(
                $"{Base}/{personId}/sea-services/{index}", ct);
            http.EnsureSuccessStatusCode();
        }
    }
}