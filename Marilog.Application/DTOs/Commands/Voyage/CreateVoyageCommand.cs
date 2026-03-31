
namespace Marilog.Application.DTOs.Commands.Voyage
{
    public record CreateVoyageCommand(
    int VesselId,
    string VoyageNumber,
    DateOnly VoyageMonth,
    int? MasterContractId = null,
    int? DeparturePortId = null,
    int? ArrivalPortId = null,
    DateTime? DepartureDate = null,
    DateTime? ArrivalDate = null,
    string? CargoType = null,
    decimal? CargoQuantityMt = null,
    string? Notes = null
);
}
