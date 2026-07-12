
namespace Marilog.Contracts.DTOs.Requests.BankDTOs
{
    public class UpdateBankRequest
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? ShortName { get; set; }

        public string? LegalName { get; set; }

        public int? ParentBankId { get; set; }

        public string? SwiftBic { get; set; }

        public string? BranchCode { get; set; }

        public string? ClearingCode { get; set; }

        public string? NationalBankCode { get; set; }

        public int CountryId { get; set; }

        public string? City { get; set; }

        public string? Address { get; set; }

        public string? Website { get; set; }

        public string? Note { get; set; }
    }
}
