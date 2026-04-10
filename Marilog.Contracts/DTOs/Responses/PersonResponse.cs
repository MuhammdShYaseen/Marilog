namespace Marilog.Contracts.DTOs.Responses
{
    public class PersonResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Nationality { get; set; } = null!;
        public string? NationalityCountryName { get; set; }
        public string? PassportNo { get; set; } = null!;
        public DateOnly? PassportExpiry { get; set; }
        public bool IsPassportExpired { get; set; }  // يحسب في الخدمة أو projection
        public string? SeamanBookNo { get; set; } = null!;
        public DateOnly? DateOfBirth { get; set; }
        public string? Phone { get; set; } = null!;
        public string? Email { get; set; } = null!;
        public string? BankName { get; set; } = null!;
        public string? IBAN { get; set; } = null!;
        public string? BankSwiftCode { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
