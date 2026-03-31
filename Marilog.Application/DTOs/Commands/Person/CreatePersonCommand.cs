

namespace Marilog.Application.DTOs.Commands.Person
{
    public record CreatePersonCommand(
    string BankName,
    string IBAN,
    bool IsPassportExpired,
    string FullName,
    string? BankSwiftCode = null,
    int? Nationality = null,
    string? PassportNo = null,
    DateOnly? PassportExpiry = null,
    string? SeamanBookNo = null,
    DateOnly? DateOfBirth = null,
    string? Phone = null,
    string? Email = null
);
}
