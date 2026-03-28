namespace Marilog.Presentation.DTOs.PersonDTOs
{
    public class UpdateBankAccountRequest
    {
        public string? BankName { get; internal set; }
        public string? IBAN { get; internal set; }
        public string? BankSwiftCode { get; internal set; }
    }
}
