namespace Marilog.Presentation.DTOs.ContractDTOs
{
    public record RemovePartyAmendmentRequest(int CompanyId, string Role, int AmendmentNumber);
}
