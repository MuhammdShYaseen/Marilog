namespace Marilog.Contracts.DTOs.Responses
{
    public class ContractPartyResponse
    {
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string Role {  get; set; } = string.Empty;
    }
}
