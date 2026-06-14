using Marilog.Kernel.Enums;


namespace Marilog.Contracts.DTOs.Responses
{
    public class BankAccountResponse
    {
        public string IBAN { get; set; } = null!;
        public string BankName { get; set; } = null!;
        public string? SwiftCode { get; set; }
        public int CurrencyId { get; set; }
        public string? AccountHolderName { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class EmailsResponse
    {
        public string Address { get; set; } = null!;
        public EmailRole Role { get; set; }
        public string? Label { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class PhonesResponse
    {
        public string Number { get; set; } = null!;
        public PhoneType Type { get; set; }
        public string? Label { get; set; }
        public bool IsPrimary { get; set; }
    }
}
