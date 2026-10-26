
namespace Marilog.Contracts.DTOs.Requests.CompanyDTOs
{
    public sealed record UpdateOwnCompanyRequest(bool IsOwnCompany);
    public sealed record OwnCompanyNameResponse(string Name);
}
