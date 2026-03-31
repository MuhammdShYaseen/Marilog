

namespace Marilog.Application.DTOs.Commands.Company
{
    public record CreateCompanyCommand(
    string? RegistrationNumber,
    string CompanyName,
    int? CountryId,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address
);
}
