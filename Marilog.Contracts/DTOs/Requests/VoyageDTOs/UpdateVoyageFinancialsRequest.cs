namespace Marilog.Contracts.DTOs.Requests.VoyageDTOs
{
    public record UpdateVoyageFinancialsRequest(
    decimal CashOnBoard,
    decimal CigarettesOnBoard,
    decimal PreviousMasterBalance
);
}
