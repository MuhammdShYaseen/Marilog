using Marilog.Application.Interfaces.DocumentTemplates;
using Marilog.Contracts.DTOs.Requests.DocumentTemplateDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class DocumentTemplateService : IDocumentTemplateService
    {
        private readonly IRepository<DocumentTemplate> _repo;
        private readonly IDocumentService _documentService;
        private readonly IDocumentTemplateRenderer _renderer;

        public DocumentTemplateService(IRepository<DocumentTemplate> repository,
            IDocumentService documentService, IDocumentTemplateRenderer renderer)
        {
            _repo = repository;
            _documentService = documentService;
            _renderer = renderer;
        }

        // ── Queries ──────────────────────────────────────────────

        public async Task<IReadOnlyList<DocumentTemplateSummaryResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .Select(t => new DocumentTemplateSummaryResponse
                {
                    Id = t.Id,
                    Name = t.Name,
                    PageSize = t.PageSize,
                    IsLandscape = t.IsLandscape,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync(ct);
        }

        public async Task<DocumentTemplateResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var template = await _repo.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            return template is null ? null : ToResponse(template);
        }

        public Task<IReadOnlyList<TemplateFontResponse>> GetFontsAsync(CancellationToken ct = default)
            => Task.FromResult(_renderer.GetFonts());

        // ── Commands ─────────────────────────────────────────────

        public async Task<DocumentTemplateResponse> CreateAsync(SaveDocumentTemplateRequest request, CancellationToken ct = default)
        {
            EnsureFontExists(request.FontFamily);
            await EnsureNameIsFreeAsync(request.Name, null, ct);

            var template = DocumentTemplate.Create(request.Name, request.PageSize, request.IsLandscape,
                request.FontFamily, ToElements(request.Elements));

            await _repo.AddAsync(template, ct);
            await _repo.SaveChangesAsync(ct);

            return ToResponse(template);
        }

        public async Task<DocumentTemplateResponse> UpdateAsync(int id, SaveDocumentTemplateRequest request, CancellationToken ct = default)
        {
            var template = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"There is no document template with ID: {id}");

            EnsureFontExists(request.FontFamily);
            await EnsureNameIsFreeAsync(request.Name, id, ct);

            template.Update(request.Name, request.PageSize, request.IsLandscape,
                request.FontFamily, ToElements(request.Elements));

            await _repo.SaveChangesAsync(ct);

            return ToResponse(template);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var template = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"There is no document template with ID: {id}");

            _repo.HardDelete(template);
            await _repo.SaveChangesAsync(ct);
        }

        // ── PDF ──────────────────────────────────────────────────

        public async Task<byte[]> ExportAsync(int templateId, int documentId, CancellationToken ct = default)
        {
            var template = await GetByIdAsync(templateId, ct)
                ?? throw new KeyNotFoundException($"There is no document template with ID: {templateId}");

            var document = await _documentService.GetWithItemsAsync(documentId, false, ct)
                ?? throw new KeyNotFoundException($"There is no document with ID: {documentId}");

            return _renderer.Render(template, document);
        }

        public async Task<byte[]> PreviewAsync(PreviewDocumentTemplateRequest request, CancellationToken ct = default)
        {
            var design = request.Template;
            EnsureFontExists(design.FontFamily);

            // Built through the entity so the preview follows exactly the same rules as saving. Nothing is stored.
            var template = DocumentTemplate.Create(design.Name, design.PageSize, design.IsLandscape,
                design.FontFamily, ToElements(design.Elements));

            DocumentResponse? document = null;
            if (request.DocumentId is { } documentId)
                document = await _documentService.GetWithItemsAsync(documentId, false, ct)
                    ?? throw new KeyNotFoundException($"There is no document with ID: {documentId}");

            return _renderer.Render(ToResponse(template), document);
        }

        // ── Private Helpers ──────────────────────────────────────

        private void EnsureFontExists(string? family)
        {
            if (string.IsNullOrWhiteSpace(family)
                || !_renderer.GetFonts().Any(f => f.Family.Equals(family.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"Font '{family}' is not one of the fonts bundled with Marilog.");
        }

        private async Task EnsureNameIsFreeAsync(string? name, int? exceptId, CancellationToken ct)
        {
            var trimmed = name?.Trim() ?? string.Empty;

            var taken = await _repo.Query()
                .AnyAsync(t => t.Name == trimmed && (exceptId == null || t.Id != exceptId), ct);

            if (taken)
                throw new InvalidOperationException($"A template named '{trimmed}' already exists.");
        }

        private static List<DocumentTemplateElement> ToElements(List<DocumentTemplateElementDto>? elements)
            => (elements ?? [])
                .Select(e => DocumentTemplateElement.Create(
                    e.Key, e.PageScope, e.X, e.Y, e.Width, e.Height, e.FontSize, e.Bold,
                    e.Align, e.Border, e.Text, e.ImageDataUrl,
                    (e.Columns ?? []).Select(c => DocumentTemplateColumn.Create(c.Key, c.Width, c.Title)).ToList()))
                .ToList();

        private static DocumentTemplateResponse ToResponse(DocumentTemplate t) => new()
        {
            Id = t.Id,
            Name = t.Name,
            PageSize = t.PageSize,
            IsLandscape = t.IsLandscape,
            FontFamily = t.FontFamily,
            UpdatedAt = t.UpdatedAt,
            Elements = t.Elements.Select(e => new DocumentTemplateElementDto
            {
                Key = e.Key,
                PageScope = e.PageScope,
                X = e.X,
                Y = e.Y,
                Width = e.Width,
                Height = e.Height,
                FontSize = e.FontSize,
                Bold = e.Bold,
                Align = e.Align,
                Border = e.Border,
                Text = e.Text,
                ImageDataUrl = e.ImageDataUrl,
                Columns = e.Columns.Select(c => new DocumentTemplateColumnDto
                {
                    Key = c.Key,
                    Width = c.Width,
                    Title = c.Title
                }).ToList()
            }).ToList()
        };
    }
}