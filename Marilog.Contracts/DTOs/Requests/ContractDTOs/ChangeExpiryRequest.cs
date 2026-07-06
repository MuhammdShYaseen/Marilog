namespace Marilog.Contracts.DTOs.Requests.ContractDTOs
{
    public record ChangeExpiryRequest(DateOnly NewExpiryDate, string ChangedBy);
}
