using Marilog.Domain.Common;

namespace Marilog.Domain.Entities.SystemEntities
{
    /// <summary>
    /// Lookup entity — replaces the enum to allow adding new types without migrations.
    /// Seed well-known types in DbContext.OnModelCreating.
    /// </summary>
    public class DocumentType : Entity
    {
        public string Code { get; private set; } = null!;   // e.g. "QUOTATION"
        public string Name { get; private set; } = null!;   // e.g. "Sales Quotation"
        public int SortOrder { get; private set; }


        private DocumentType() { }

        public static DocumentType Create(string code, string name, int sortOrder = 0)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return new DocumentType
            {
                Code = code.ToUpperInvariant(),
                Name = name,
                SortOrder = sortOrder
            };
        }

        public void Update(string name, int sortOrder)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Name = name;
            SortOrder = sortOrder;
            Touch();
        }
    }
}