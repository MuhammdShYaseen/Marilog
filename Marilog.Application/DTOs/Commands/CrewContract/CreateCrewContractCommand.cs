
namespace Marilog.Application.DTOs.Commands.CrewContract
{
    public record CreateCrewContractCommand(
     int PersonId,
     int VesselId,
     int RankId,
     decimal MonthlyWage,
     DateOnly SignOnDate,
     int? SignOnPort,
     string? Notes
 );
}
