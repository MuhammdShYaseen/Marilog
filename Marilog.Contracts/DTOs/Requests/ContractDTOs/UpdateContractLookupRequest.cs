namespace Marilog.Contracts.DTOs.Requests.ContractLookupDTOs
{
    /// <summary>Used by both contract types and contract roles. Code is not editable.</summary>
    public class UpdateContractLookupRequest
    {
        public string Name { get; set; } = default!;
        public int SortOrder { get; set; }
    }
}
