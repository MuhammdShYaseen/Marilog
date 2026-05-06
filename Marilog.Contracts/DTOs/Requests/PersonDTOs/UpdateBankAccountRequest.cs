namespace Marilog.Contracts.DTOs.Requests.PersonDTOs
{
    public class UpdateBankAccountRequest
    {
        public string? BankName { get; init; }
        public string? IBAN { get; init; }
        public string? BankSwiftCode { get; init; }
    }
}
