namespace Marilog.Contracts.DTOs.Requests.CompanyDTOs
{
    public class UpdateCompanyRequest
    {
        public string CompanyName { get; set; } = default!;
        public int? CountryId { get; set; }
        public string? ContactName { get; set; }
        public string? Address { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? WebSite { get; set; }
    }
}
