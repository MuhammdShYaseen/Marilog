using Marilog.Client.Extensions;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.CompanyDTOs;
using Marilog.Contracts.DTOs.Requests.ContactsRequestDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Kernel.Enums;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class CompanyService : ICompanyService
    {
        private readonly HttpClient _http;
        private const string Base = "api/companies";

        public CompanyService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<CompanyResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _http.GetApiAsync<CompanyResponse>($"{Base}/{id}", ct);
        }

        public async Task<CompanyResponse?> GetWithVesselsAsync(int id, CancellationToken ct = default)
        {
            return await _http.GetApiAsync<CompanyResponse>($"{Base}/{id}/with-vessels", ct);
        }

        public async Task<IReadOnlyList<CompanyResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await _http.GetApiListAsync<CompanyResponse>(Base, ct);
          
        }

        public async Task<IReadOnlyList<CompanyResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _http.GetApiListAsync<CompanyResponse>($"{Base}/active", ct);
        }

        public async Task<IReadOnlyList<CompanyResponse>> SearchByNameAsync(string name, CancellationToken ct = default)
        {
            return await _http.GetApiListAsync<CompanyResponse>($"{Base}/search?name={Uri.EscapeDataString(name)}", ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<CompanyResponse> CreateAsync(string? registrationNumber, string? webSite, string companyName, int? countryId = null,
            string? contactName = null, string? address = null,
            CancellationToken ct = default)
        {
            var request = new CreateCompanyRequest
            {
                RegistrationNumber = registrationNumber,
                CompanyName = companyName,
                CountryId = countryId,
                ContactName = contactName,
                Address = address,
                WebSite = webSite,
            };

            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();

            var response = await http.Content.ReadFromJsonAsync<ApiResponse<CompanyResponse>>(ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<CompanyResponse>> CreateRangeAsync(IEnumerable<CreateCompanyRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", commands, ct);
            http.EnsureSuccessStatusCode();

            var response = await http.Content.ReadFromJsonAsync<IReadOnlyList<CompanyResponse>>(ct);
            return response ?? [];
        }

        public async Task UpdateAsync(int id, string? registerationNumber, string? website, string companyName, int? countryId = null,
            string? contactName = null, string? address = null,
            CancellationToken ct = default)
        {
            var request = new UpdateCompanyRequest
            {
                CompanyName = companyName,
                CountryId = countryId,
                ContactName = contactName,
                Address = address,
                RegistrationNumber = registerationNumber,
                WebSite = website,
                
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

        // ── Bank Accounts ─────────────────────────────────────────────────────────

        public async Task AddBankAccountAsync(int companyId, string iban, string bankName, string? swiftCode,
            int currencyId, string? accountHolderName, bool isPrimary, CancellationToken ct = default)
        {
            var request = new AddBankAccountRequest
            {
                IBAN = iban,
                BankName = bankName,
                SwiftCode = swiftCode,
                CurrencyId = currencyId,
                AccountHolderName = accountHolderName,
                IsPrimary = isPrimary
            };
            var http = await _http.PostAsJsonAsync($"{Base}/{companyId}/bank-accounts", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task UpdateBankAccountAsync(int companyId, string iban, string bankName, string? swiftCode,
            int currencyId, string? accountHolderName, bool isPrimary, CancellationToken ct = default)
        {
            var request = new AddBankAccountRequest
            {
                IBAN = iban,
                BankName = bankName,
                SwiftCode = swiftCode,
                CurrencyId = currencyId,
                AccountHolderName = accountHolderName,
                IsPrimary = isPrimary
            };
            var http = await _http.PutAsJsonAsync($"{Base}/{companyId}/bank-accounts/{Uri.EscapeDataString(iban)}", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task RemoveBankAccountAsync(int companyId, string iban, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{companyId}/bank-accounts/{Uri.EscapeDataString(iban)}", ct);
            http.EnsureSuccessStatusCode();
        }

        // ── Emails ────────────────────────────────────────────────────────────────

        public async Task AddEmailAsync(int companyId, string address, EmailRole role, string? label,
            bool isPrimary, CancellationToken ct = default)
        {
            var request = new AddEmailRequest
            {
                Address = address,
                Role = role,
                Label = label,
                IsPrimary = isPrimary
            };
            var http = await _http.PostAsJsonAsync($"{Base}/{companyId}/emails", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task UpdateEmailAsync(int companyId, string oldAddress, string newAddress, EmailRole role,
            string? label, bool isPrimary, CancellationToken ct = default)
        {
            var request = new AddEmailRequest
            {
                Address = newAddress,
                Role = role,
                Label = label,
                IsPrimary = isPrimary
            };
            var http = await _http.PutAsJsonAsync($"{Base}/{companyId}/emails/{Uri.EscapeDataString(oldAddress)}", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task RemoveEmailAsync(int companyId, string address, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{companyId}/emails/{Uri.EscapeDataString(address)}", ct);
            http.EnsureSuccessStatusCode();
        }

        // ── Phones ────────────────────────────────────────────────────────────────

        public async Task AddPhoneAsync(int companyId, string number, PhoneType type, string? label,
            bool isPrimary, CancellationToken ct = default)
        {
            var request = new AddPhoneRequest
            {
                Number = number,
                Type = type,
                Label = label,
                IsPrimary = isPrimary
            };
            var http = await _http.PostAsJsonAsync($"{Base}/{companyId}/phones", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task UpdatePhoneAsync(int companyId, string oldNumber, PhoneType oldType, string newNumber,
            PhoneType newType, string? label, bool isPrimary, CancellationToken ct = default)
        {
            var request = new UpdatePhoneRequest
            {
                OldNumber = oldNumber,
                OldType = oldType,
                NewNumber = newNumber,
                NewType = newType,
                Label = label,
                IsPrimary = isPrimary
            };
            var http = await _http.PutAsJsonAsync($"{Base}/{companyId}/phones", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task RemovePhoneAsync(int companyId, string number, PhoneType type, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync(
                $"{Base}/{companyId}/phones?number={Uri.EscapeDataString(number)}&type={type}", ct);
            http.EnsureSuccessStatusCode();
        }
    }
}