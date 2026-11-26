using Marilog.Domain.Common;

namespace Marilog.Ship.Domain.Entities
{
    /// <summary>Lookup for "Other" crew deductions (overpayment recovery, sign-off cost, ...).</summary>
    public class DeductionType : Entity
    {
        public string Code { get; private set; } = null!;
        public string Name { get; private set; } = null!;

        private DeductionType() { }

        public static DeductionType Create(string code, string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return new DeductionType
            {
                Code = code.Trim().ToUpperInvariant(),
                Name = name.Trim()
            };
        }

        public void Update(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Name = name.Trim();
            Touch();
        }
    }
}