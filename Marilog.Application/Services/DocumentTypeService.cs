using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.Interfaces.Services;

namespace Marilog.Application.Services
{
    public class DocumentTypeService : IDocumentTypeService
    {
        private readonly IRepository<DocumentType> _repo;

        public DocumentTypeService(IRepository<DocumentType> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<DocumentType?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.GetByIdAsync(id, ct);

        public async Task<DocumentType?> GetByCodeAsync(string code, CancellationToken ct = default)
            => await _repo.Query()
                          .FirstOrDefaultAsync(x => x.Code == code.ToUpperInvariant(), ct);

        public async Task<IReadOnlyList<DocumentType>> GetAllAsync(CancellationToken ct = default)
            => await _repo.Query()
                          .OrderBy(x => x.SortOrder)
                          .ThenBy(x => x.Name)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<DocumentType>> GetActiveAsync(CancellationToken ct = default)
            => await _repo.Query()
                          .Where(x => x.IsActive)
                          .OrderBy(x => x.SortOrder)
                          .ThenBy(x => x.Name)
                          .ToListAsync(ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<DocumentType> CreateAsync(string code, string name,
            int sortOrder = 0, CancellationToken ct = default)
        {
            var exists = await _repo.Query()
                .AnyAsync(x => x.Code == code.ToUpperInvariant(), ct);
            if (exists)
                throw new InvalidOperationException($"Document type code '{code}' already exists.");

            var docType = DocumentType.Create(code, name, sortOrder);
            await _repo.AddAsync(docType, ct);
            await _repo.SaveChangesAsync(ct);
            return docType;
        }

        public async Task UpdateAsync(int id, string name, int sortOrder,
            CancellationToken ct = default)
        {
            var docType = await GetOrThrowAsync(id, ct);
            docType.Update(name, sortOrder);
            _repo.Update(docType);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var docType = await GetOrThrowAsync(id, ct);
            docType.Activate();
            _repo.Update(docType);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var docType = await GetOrThrowAsync(id, ct);
            docType.Deactivate();
            _repo.Update(docType);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var docType = await GetOrThrowAsync(id, ct);
            _repo.HardDelete(docType);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<DocumentType> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"DocumentType {id} not found.");
    }
}
