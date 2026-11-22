using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Application.Interfaces.DocumentTemplates
{
    // What a template is filled with: the document, plus the few details DocumentResponse doesn't carry.
    public sealed class DocumentTemplateData
    {
        public required DocumentResponse Document { get; init; }
        public TemplateCompany? Supplier { get; init; }
        public TemplateCompany? Buyer { get; init; }
        public string? CurrencyName { get; init; }     // e.g. "US Dollar", for the amount in words
    }

    // Email, phone and bank account: the primary one, or the first if none is marked primary.
    // The bank account in the document's currency comes first, so the invoice shows where to pay that currency.
    public sealed record TemplateCompany(string? Address, string? Country, string? RegistrationNumber, string? ContactName,
        string? Email, string? Phone, TemplateBankAccount? BankAccount);

    public sealed record TemplateBankAccount(string BankName, string? AccountHolderName, string Iban, string? SwiftCode);
}