using Marilog.Domain.Common;

namespace Marilog.Domain.Entities.SystemEntities
{
    /// <summary>
    /// Lookup entity — user-defined party roles (Owner, Charterer, Broker, ...).
    /// </summary>
    public class ContractRole : Entity
    {
        public string Code { get; private set; } = null!;   // e.g. "CHRT"
        public string Name { get; private set; } = null!;   // e.g. "Charterer"
        public int SortOrder { get; private set; }

        private ContractRole() { }

        public static ContractRole Create(string code, string name, int sortOrder = 0)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return new ContractRole
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
