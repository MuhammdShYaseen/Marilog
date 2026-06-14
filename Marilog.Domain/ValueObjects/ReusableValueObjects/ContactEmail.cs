
using Marilog.Kernel.Enums;

namespace Marilog.Domain.ValueObjects.ReusableValueObjects
{
    // Marilog.Domain/ValueObjects/ContactEmail.cs
    public sealed class ContactEmail : ValueObject
    {
        public string Address { get; private set; } = null!;
        public EmailRole Role { get; private set; }  // Operations, Accounts, Legal, General
        public string? Label { get; private set; }   // custom label e.g. "Captain Direct"
        public bool IsPrimary { get; private set; }

        private ContactEmail() { }

        public static ContactEmail Create(string address, EmailRole role, string label, bool isPrimary)
        {
            if(string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address));
            return new ContactEmail
            {

                    Address = address,
                    Role = role,
                    Label= label,
                    IsPrimary = isPrimary
            };
        }

        public void Update(string address, EmailRole role, string label, bool isPrimary)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address));


            Address = address;
            Role = role;
            Label = label;
            IsPrimary = isPrimary;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Address;
            yield return Role;
        }
    }
    
}
