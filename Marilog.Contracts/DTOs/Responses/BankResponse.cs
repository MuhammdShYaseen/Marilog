
namespace Marilog.Contracts.DTOs.Responses
{
    public class BankResponse
    {
        public string? BankName { get; set; }
        public string? LegalName { get; set; }
        public string? Country { get; set; }
        public string? SwiftBic { get; set; }
        public string? ShortName { get; set; }
        public int ParentBankId { get; set; }
        public string? BranchCode { get; set; }
        public string? ClearingCode { get; set; }
        public string? NationalBankCode{ get; set; }
        public int? CountryId { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? Notes { get; set; }
        public string? Branches { get; set; }
        public List<PhonesResponse>? Phones { get; set; }
        public List<EmailsResponse>? Emails { get; set; }
        public string? CountryName { get; set; }
        public IReadOnlyList<BankResponse>? Branchs { get; set; }
    }
}
