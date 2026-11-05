namespace Marilog.Contracts.DTOs.Responses
{
    /// <summary>Used by both contract types and contract roles.</summary>
    public class ContractLookupResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
