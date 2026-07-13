using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.BankDTOs;
using Marilog.Contracts.DTOs.Requests.ContactsRequestDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Primitives;


namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IBankService
    {
        // Query
        Task<BankResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedResponse<BankResponse>> GetPagedAsync(bool treeMode = false, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BankResponse>> GetAllActiveAsync(bool treeMode = false, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BankResponse>> GetByCountryIdAsync(int countryId, bool treeMode = false, CancellationToken cancellationToken = default);
        Task<List<LookupItem<int>>> GetLookupAsync(CancellationToken cancellationToken = default);

        Task<List<LookupItem<int>>> GetParentBanksLookupAsync(int? excludeBankId = null, CancellationToken cancellationToken = default);

        // Commands
        Task<BankResponse> CreateAsync(CreateBankRequest request, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BankResponse>> CreateRangAsync(List<CreateBankRequest> requests , CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(UpdateBankRequest request, CancellationToken cancellationToken = default);
        Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> ActivateAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);


        // Email Management
        Task<Result> AddEmailAsync(int bankId, AddEmailRequest request, CancellationToken cancellationToken = default);

        Task<Result> RemoveEmailAsync(int bankId, string email, CancellationToken cancellationToken = default);

        // Phone Management
        Task<Result> AddPhoneAsync(int bankId, AddPhoneRequest request, CancellationToken cancellationToken = default);

        Task<Result> RemovePhoneAsync(int bankId, string phoneNumber, CancellationToken cancellationToken = default);
    }
}
