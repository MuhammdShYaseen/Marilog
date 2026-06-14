namespace Marilog.Contracts.DTOs.Responses
{
    public class CompanyResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? RegistrationNumber { get; set; }  // حقل فارغ كما في المثال
        public string? ContactName { get; set; } = null!;
        public List<BankAccountResponse> BankAccounts { get; set; } = [];
        public List<EmailsResponse> Emails { get; set; } = [];
        public List<PhonesResponse> Phones { get; set; } = [];
        public string? Address { get; set; } = null!;
        public bool IsActive { get; set; }
        public int? CountryId { get; set; }
        public List<VesselResponse> Vessels { get; set; } = new List<VesselResponse>();
        public string? WebSite { get; set; }
    }
}
