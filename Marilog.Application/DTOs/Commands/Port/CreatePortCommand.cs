

namespace Marilog.Application.DTOs.Commands.Port
{
    public record CreatePortCommand
    (
        string PortCode,
        string PortName,
        int? CountryId = null
    );
}
