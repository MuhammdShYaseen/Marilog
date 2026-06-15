using Marilog.Contracts.DTOs.Requests.CompanyDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface ICompanyService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<CompanyResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<CompanyResponse?>              GetWithVesselsAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<CompanyResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<CompanyResponse>> GetActiveAsync(CancellationToken ct = default);
        Task<IReadOnlyList<CompanyResponse>> SearchByNameAsync(string name, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<CompanyResponse> CreateAsync(string? registrationNumber, string? website, string companyName, int? countryId = null,
                                  string? contactName = null,
                                  string? address = null,
                                  CancellationToken ct = default);

        Task<IReadOnlyList<CompanyResponse>> CreateRangeAsync(IEnumerable<CreateCompanyRequest> commands, CancellationToken ct = default);
        Task          UpdateAsync(int id, string? registrationNumber, string? website, string companyName, int? countryId = null,
                                  string? contactName = null, 
                                  string? address = null,
                                  CancellationToken ct = default);
        Task          ActivateAsync(int id, CancellationToken ct = default);
        Task          DeactivateAsync(int id, CancellationToken ct = default);
        Task          DeleteAsync(int id, CancellationToken ct = default);


        // ── Bank Accounts ─────────────────────────────────────────────────────────
        Task AddBankAccountAsync(int companyId, string iban, string bankName, string? swiftCode,
                                 int currencyId, string? accountHolderName, bool isPrimary,
                                 CancellationToken ct = default);
        Task UpdateBankAccountAsync(int companyId, string oldIban, string? newIban, string bankName, string? swiftCode,
                                    int currencyId, string? accountHolderName, bool isPrimary,
                                    CancellationToken ct = default);
        Task RemoveBankAccountAsync(int companyId, string iban, CancellationToken ct = default);

        // ── Emails ────────────────────────────────────────────────────────────────
        Task AddEmailAsync(int companyId, string address, EmailRole role, string? label, bool isPrimary,
                           CancellationToken ct = default);
        Task UpdateEmailAsync(int companyId, string oldAddress, string newAddress, EmailRole role, string? label, bool isPrimary,
                              CancellationToken ct = default);
        Task RemoveEmailAsync(int companyId, string address, CancellationToken ct = default);

        // ── Phones ────────────────────────────────────────────────────────────────
        Task AddPhoneAsync(int companyId, string number, PhoneType type, string? label, bool isPrimary,
                           CancellationToken ct = default);
        Task UpdatePhoneAsync(int companyId, string oldNumber, PhoneType oldType, string newNumber, PhoneType newType, string? label, bool isPrimary,
                              CancellationToken ct = default);
        Task RemovePhoneAsync(int companyId, string number, PhoneType type, CancellationToken ct = default);
    }
}
