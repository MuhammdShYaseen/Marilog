using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Application.Interfaces.Services;

namespace Marilog.Application.Services
{
    public class DocumentNumberService : IDocumentNumberService
    {
        private readonly IRepository<Document> _documentRepo;

        // ── Prefix map: DocType.Code → prefix ────────────────────────────────────
        private static readonly Dictionary<string, string> _prefixMap = new()
        {
            { "QUOTATION",     "QT"  },
            { "DELIVERY_NOTE", "DN"  },
            { "TAX_INVOICE",   "INV" },
        };

        private const string DefaultPrefix = "DOC";

        public DocumentNumberService(IRepository<Document> documentRepo)
            => _documentRepo = documentRepo;

        /// <summary>
        /// Generates the next sequential document number for the given type.
        /// Format: {PREFIX}-{YEAR}-{SEQUENCE:D4}
        /// Example: INV-2025-0042
        ///
        /// Sequence resets each year per document type.
        /// </summary>
        public async Task<string> GenerateAsync(DocumentType docType,
            CancellationToken ct = default)
        {
            if (docType is null) throw new ArgumentNullException(nameof(docType));

            var prefix  = _prefixMap.TryGetValue(docType.Code, out var mapped)
                              ? mapped
                              : DefaultPrefix;
            var year    = DateTime.UtcNow.Year;
            var pattern = $"{prefix}-{year}-";

            // Find last used sequence for this prefix + year
            var lastNumber = await _documentRepo.Query()
                .Where(x => x.DocNumber.StartsWith(pattern))
                .Select(x => x.DocNumber)
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync(ct);

            var nextSequence = 1;

            if (lastNumber is not null)
            {
                // Extract sequence from e.g. "INV-2025-0042" → 42
                var parts = lastNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var last))
                    nextSequence = last + 1;
            }

            return $"{pattern}{nextSequence:D4}";
        }
    }
}
