namespace Marilog.Presentation.DTOs.ContractDTOs
{
    public record ExtendExpiryRequest(DateOnly NewExpiryDate, int AmendmentNumber);
}
