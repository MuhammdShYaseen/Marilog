

namespace Marilog.Application.DTOs.Commands.Office
{
    public record CreateOfficeCommand(
    string OfficeName,
    string City,
    int CountryId,
    string? Address = null,
    string? Phone = null,
    string? ContactName = null
);
}
