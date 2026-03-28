namespace Marilog.Presentation.DTOs.PersonDTOs
{
    public class CreatePersonRequest
    {
        public string BankName { get; internal set; } = string.Empty;
        public string IBAN { get; internal set; } = string.Empty ;
        public bool IsPassportExpired { get; internal set; }
        public string? BankSwiftCode { get; internal set; }
        public string FullName { get; internal set; } = string.Empty;
        public int? Nationality { get; internal set; }
        public string? PassportNo { get; internal set; }
        public DateOnly? PassportExpiry { get; internal set; }
        public string? SeamanBookNo { get; internal set; }
        public DateOnly? DateOfBirth { get; internal set; }
        public string? Phone { get; internal set; }
        public string? Email { get; internal set; }
    }
}
