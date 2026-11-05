namespace Marilog.Contracts.DTOs.Requests.ContractLookupDTOs
{
    /// <summary>Used by both contract types and contract roles.</summary>
    public class CreateContractLookupRequest
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int SortOrder { get; set; } = 0;
    }
}
