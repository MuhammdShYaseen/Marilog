
using Marilog.Kernel.Enums;

namespace Marilog.Domain.ValueObjects.ReusableValueObjects
{
    public sealed class ContactPhone : ValueObject
    {
        public string Number { get; private set; } = null!;
        public PhoneType Type { get; private set; }   // Office, Mobile, Fax, WhatsApp
        public string? Label { get; private set; }
        public bool IsPrimary { get; private set; }

        private ContactPhone() { }

        public static ContactPhone Create(string number, PhoneType type, string? lable, bool isPrimary)
        {
            if(string.IsNullOrWhiteSpace(number))
                throw new ArgumentNullException(nameof(number));

            return new ContactPhone
            {
                Number = number,
                Type = type,
                Label = lable,
                IsPrimary = isPrimary
            };
        }


        public void Update(string number, PhoneType type, string? lable, bool isPrimary)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentNullException(nameof(number));


            Number = number;
            Type = type;
            Label = lable;
            IsPrimary = isPrimary;
            
        }
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Number;
            yield return Type;
        }
    }
}
