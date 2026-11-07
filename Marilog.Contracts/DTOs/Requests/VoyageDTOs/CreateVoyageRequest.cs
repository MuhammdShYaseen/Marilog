namespace Marilog.Contracts.DTOs.Requests.VoyageDTOs
{
    public record CreateVoyageRequest(
        int VesselId,
        string VoyageNumber,
        DateOnly VoyageMonth,
        int? MasterContractId = null,
        string? Notes = null
    );
}
