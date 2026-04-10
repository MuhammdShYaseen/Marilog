namespace Marilog.Contracts.DTOs.Requests.PersonDTOs
{
    public class UpdatePersonRequest
    {
        public string FullName { get;  set; } = null!;
        public int? Nationality { get;  set; }
        public string? PassportNo { get;  set; }
        public DateOnly? PassportExpiry { get;  set; }
        public string? SeamanBookNo { get;  set; }
        public DateOnly? DateOfBirth { get;  set; }
        public string? Phone { get;  set; }
        public string? Email { get;  set; }
        public string? BankName { get; set; } = null!;
        public string? IBAN { get; set; } = null!;
        public string? BankSwiftCode { get; set; } = null!;
    }
}
