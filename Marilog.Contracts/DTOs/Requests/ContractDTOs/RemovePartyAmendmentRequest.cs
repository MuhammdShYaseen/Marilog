namespace Marilog.Contracts.DTOs.Requests.ContractDTOs
{
    public record RemovePartyAmendmentRequest(int CompanyId, string Role, int AmendmentNumber);
}
