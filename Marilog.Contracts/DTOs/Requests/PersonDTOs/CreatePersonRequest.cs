namespace Marilog.Contracts.DTOs.Requests.PersonDTOs
{
    public class CreatePersonRequest
    {
        public string BankName { get;  set; } = string.Empty;
        public string IBAN { get;  set; } = string.Empty ;
        public bool IsPassportExpired { get;  set; }
        public string? BankSwiftCode { get;  set; }
        public string FullName { get; set; } = string.Empty;
        public int? Nationality { get; set; }
        public string? PassportNo { get; set; }
        public DateOnly? PassportExpiry { get; set; }
        public string? SeamanBookNo { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}
