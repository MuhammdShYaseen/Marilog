namespace Marilog.Presentation.DTOs.VoyageDTOs
{
    public record UpdateVoyageFinancialsRequest(
    decimal CashOnBoard,
    decimal CigarettesOnBoard,
    decimal PreviousMasterBalance
);
}
