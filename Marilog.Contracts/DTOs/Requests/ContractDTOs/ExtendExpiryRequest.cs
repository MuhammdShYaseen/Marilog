namespace Marilog.Contracts.DTOs.Requests.ContractDTOs
{
    public record ExtendExpiryRequest(DateOnly NewExpiryDate, int AmendmentNumber);
}
