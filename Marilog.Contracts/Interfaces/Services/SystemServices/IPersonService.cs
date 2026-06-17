using Marilog.Contracts.DTOs.Requests.PersonDTOs;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IPersonService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<PersonResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<PersonResponse?>              GetByPassportAsync(string passportNo, CancellationToken ct = default);
        Task<PersonResponse?>              GetBySeamanBookAsync(string seamanBookNo, CancellationToken ct = default);
        Task<IReadOnlyList<PersonResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<PersonResponse>> GetActiveAsync(CancellationToken ct = default);
        Task<IReadOnlyList<PersonResponse>> SearchAsync(string term, CancellationToken ct = default);
        Task<IReadOnlyList<PersonResponse>> GetWithExpiringPassportsAsync(int withinDays, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<PersonResponse> CreateAsync(string? bankName, string? iBAN, bool isPassportExpired, string? bankSwiftCode, string fullName, int? nationality = null,
            string? passportNo = null, DateOnly? passportExpiry = null,
            string? seamanBookNo = null, DateOnly? dateOfBirth = null,
            string? phone = null, string? email = null,
            CancellationToken ct = default);

        Task<IReadOnlyList<PersonResponse>> CreateRangeAsync(IEnumerable<CreatePersonRequest> commands, CancellationToken ct = default);
        Task         UpdateAsync(int id, string fullName, int? nationality = null,
                                 string? passportNo = null, DateOnly? passportExpiry = null,
                                 string? seamanBookNo = null, DateOnly? dateOfBirth = null,
                                 string? phone = null, string? email = null,
                                 CancellationToken ct = default);
        Task         ActivateAsync(int id, CancellationToken ct = default);
        Task         DeactivateAsync(int id, CancellationToken ct = default);
        Task         DeleteAsync(int id, CancellationToken ct = default);

        // ── Bank Account ──────────────────────────────────────────────────────────
        Task UpdateBankAccountAsync(int id, string? bankName, string? iban,
                                    string? bankSwiftCode, CancellationToken ct = default);
        Task ClearBankAccountAsync(int id, CancellationToken ct = default);


        // Certificates
        Task AddCertificateAsync(int personId, UpsertCertificateRequest request, CancellationToken ct = default);
        Task UpdateCertificateAsync(int personId, int index, UpsertCertificateRequest request, CancellationToken ct = default);
        Task RemoveCertificateAsync(int personId, int index, CancellationToken ct = default);

        // Sea Services
        Task AddSeaServiceAsync(int personId, UpsertSeaServiceRequest request, CancellationToken ct = default);
        Task UpdateSeaServiceAsync(int personId, int index, UpsertSeaServiceRequest request, CancellationToken ct = default);
        Task RemoveSeaServiceAsync(int personId, int index, CancellationToken ct = default);
    }
}
