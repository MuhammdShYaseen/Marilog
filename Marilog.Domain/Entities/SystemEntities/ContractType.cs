using Marilog.Domain.Common;

namespace Marilog.Domain.Entities.SystemEntities
{
    /// <summary>
    /// Lookup entity — user-defined contract types (same idea as DocumentType).
    /// </summary>
    public class ContractType : Entity
    {
        public string Code { get; private set; } = null!;   // e.g. "CP"
        public string Name { get; private set; } = null!;   // e.g. "Charter Party"
        public int SortOrder { get; private set; }

        private ContractType() { }

        public static ContractType Create(string code, string name, int sortOrder = 0)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return new ContractType
            {
                Code = code.Trim().ToUpperInvariant(),
                Name = name.Trim(),
                SortOrder = sortOrder
            };
        }

        public void Update(string name, int sortOrder)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Name = name.Trim();
            SortOrder = sortOrder;
            Touch();
        }
    }
}
