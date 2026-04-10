namespace Marilog.Contracts.DTOs.Requests.PersonDTOs
{
    public class UpdateBankAccountRequest
    {
        public string? BankName { get; internal set; }
        public string? IBAN { get; internal set; }
        public string? BankSwiftCode { get; internal set; }
    }
}
