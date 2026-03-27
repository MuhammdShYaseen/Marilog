using Marilog.Application.DTOs;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services
{
    public class DocumentTypeService : IDocumentTypeService
    {
        private readonly IRepository<DocumentType> _repo;

        public DocumentTypeService(IRepository<DocumentType> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<DocumentTypeResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<DocumentTypeResponse?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            var upper = code.ToUpperInvariant();

            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Code == upper)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }
        public async Task<IReadOnlyList<DocumentTypeResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<DocumentTypeResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<DocumentTypeResponse> CreateAsync(string code, string name,
            int sortOrder = 0, CancellationToken ct = default)
        {
            var exists = await _repo.Query()
                .AnyAsync(x => x.Code == code.ToUpperInvariant(), ct);
            if (exists)
                throw new InvalidOperationException($"Document type code '{code}' already exists.");

            var docType = DocumentType.Create(code, name, sortOrder);
            await _repo.AddAsync(docType, ct);
            await _repo.SaveChangesAsync(ct);
            return new DocumentTypeResponse
            {
                Id = docType.Id,
                Code = code,
                Name = name,
                SortOrder = sortOrder,
            };
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

        private static readonly Expression<Func<DocumentType, DocumentTypeResponse>> ToResponse =
        x => new DocumentTypeResponse
        {
            Code = x.Code,
            Name = x.Name,
            SortOrder = x.SortOrder,
            Id = x.Id,
        };
    }
}
