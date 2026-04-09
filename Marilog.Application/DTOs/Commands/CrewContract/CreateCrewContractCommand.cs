
namespace Marilog.Application.DTOs.Commands.CrewContract
{
    public record CreateCrewContractCommand(int durationInMonth,
     int PersonId,
     int VesselId,
     int RankId,
     decimal MonthlyWage,
     DateOnly SignOnDate,
     int? SignOnPort,
     string? Notes
 );
}
