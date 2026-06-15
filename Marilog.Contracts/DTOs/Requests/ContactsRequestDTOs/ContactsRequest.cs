using Marilog.Kernel.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.DTOs.Requests.ContactsRequestDTOs
{
    public class AddBankAccountRequest
    {
        public string IBAN { get; set; } = null!;
        public string BankName { get; set; } = null!;
        public string? SwiftCode { get; set; }
        public int CurrencyId { get; set; }
        public string? AccountHolderName { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class UpdateBankAccountRequest
    {
        public string OldIBAN { get; set; } = null!;
        public string? NewIban { get; set; }
        public string BankName { get; set; } = null!;
        public string? SwiftCode { get; set; }
        public int CurrencyId { get; set; }
        public string? AccountHolderName { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class AddEmailRequest
    {
        public string Address { get; set; } = null!;
        public EmailRole Role { get; set; }
        public string? Label { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class AddPhoneRequest
    {
        public string Number { get; set; } = null!;
        public PhoneType Type { get; set; }
        public string? Label { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class UpdatePhoneRequest
    {
        public string OldNumber { get; set; } = null!;
        public PhoneType OldType { get; set; }
        public string NewNumber { get; set; } = null!;
        public PhoneType NewType { get; set; }
        public string? Label { get; set; }
        public bool IsPrimary { get; set; }
    }
}
