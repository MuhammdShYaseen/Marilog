using Marilog.Application.Interfaces.DocumentTemplates;
using Marilog.Contracts.DTOs.Requests.DocumentTemplateDTOs;
using Marilog.Contracts.DTOs.Requests.StoregFileDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Marilog.Kernel.Rules;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    // Template pictures live in StoredFiles (EntityType.DocumentTemplate, EntityId = template ID).
    // The designer sends a new picture as a data URL; it is uploaded on save and the template keeps only its file ID.
    public class DocumentTemplateService : IDocumentTemplateService
    {
        private readonly IRepository<DocumentTemplate> _repo;
        private readonly IRepository<Document> _documents;
        private readonly IDocumentService _documentService;
        private readonly IStoredFileService _storedFiles;
        private readonly IDocumentTemplateRenderer _renderer;

        public DocumentTemplateService(IRepository<DocumentTemplate> repository, IRepository<Document> documents,
            IDocumentService documentService, IStoredFileService storedFiles, IDocumentTemplateRenderer renderer)
        {
            _repo = repository;
            _documents = documents;
            _documentService = documentService;
            _storedFiles = storedFiles;
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

            request.Elements ??= [];
            EnsureImagesHaveFiles(request.Elements, ownFileIds: []);
            Build(request);   // checks the whole design before anything is uploaded

            var uploaded = await UploadNewImagesAsync(request, templateId: null, ct);

            var template = Build(request);
            await _repo.AddAsync(template, ct);
            await _repo.SaveChangesAsync(ct);

            // The template had no ID when its pictures were uploaded.
            foreach (var fileId in uploaded)
                await _storedFiles.UpdateEntityLinkAsync(fileId, EntityType.DocumentTemplate, template.Id, ct);

            return ToResponse(template);
        }

        public async Task<DocumentTemplateResponse> UpdateAsync(int id, SaveDocumentTemplateRequest request, CancellationToken ct = default)
        {
            var template = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"There is no document template with ID: {id}");

            EnsureFontExists(request.FontFamily);
            await EnsureNameIsFreeAsync(request.Name, id, ct);

            request.Elements ??= [];
            var ownFileIds = await GetFileIdsAsync(id, ct);
            EnsureImagesHaveFiles(request.Elements, ownFileIds);
            Build(request);   // checks the whole design before anything is uploaded

            await UploadNewImagesAsync(request, id, ct);

            template.Update(request.Name, request.PageSize, request.IsLandscape,
                request.FontFamily, ToElements(request.Elements));

            await _repo.SaveChangesAsync(ct);

            // Pictures removed or replaced in the designer.
            var used = request.Elements.Select(e => e.StoredFileId).OfType<int>().ToHashSet();
            foreach (var fileId in ownFileIds.Where(f => !used.Contains(f)))
                await _storedFiles.DeleteAsync(fileId, ct);

            return ToResponse(template);
        }

        // A new template with the same design, named "<name> (copy)".
        // The copy gets its own picture files, so changing or deleting one template never touches the other's pictures.
        public async Task<DocumentTemplateResponse> DuplicateAsync(int id, CancellationToken ct = default)
        {
            var source = await GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"There is no document template with ID: {id}");

            foreach (var e in source.Elements.Where(e => e.Key == TemplateElementKey.Image && e.StoredFileId is not null))
            {
                var file = await _storedFiles.GetByIdAsync(e.StoredFileId!.Value, ct);
                var bytes = await ReadImageFileAsync(e.StoredFileId.Value, ct);
                if (file is null || bytes is null)
                    throw new InvalidOperationException(
                        "A picture of this template is missing. Open the template, replace the picture, then copy it.");

                // Sent as a new picture: CreateAsync uploads it as the copy's own file.
                e.ImageDataUrl = $"data:{file.ContentType};base64,{Convert.ToBase64String(bytes)}";
                e.StoredFileId = null;
            }

            return await CreateAsync(new SaveDocumentTemplateRequest
            {
                Name = await CopyNameAsync(source.Name, ct),
                PageSize = source.PageSize,
                IsLandscape = source.IsLandscape,
                FontFamily = source.FontFamily,
                Elements = source.Elements
            }, ct);
        }

        // "Invoice (copy)", then "Invoice (copy 2)", ... within the 100-character name limit.
        private async Task<string> CopyNameAsync(string name, CancellationToken ct)
        {
            var taken = (await _repo.Query().AsNoTracking().Select(t => t.Name).ToListAsync(ct))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (var n = 1; ; n++)
            {
                var suffix = n == 1 ? " (copy)" : $" (copy {n})";
                var candidate = name[..Math.Min(name.Length, 100 - suffix.Length)] + suffix;
                if (!taken.Contains(candidate)) return candidate;
            }
        }
        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var template = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"There is no document template with ID: {id}");

            var ownFileIds = await GetFileIdsAsync(id, ct);

            _repo.HardDelete(template);
            await _repo.SaveChangesAsync(ct);

            foreach (var fileId in ownFileIds)
                await _storedFiles.DeleteAsync(fileId, ct);
        }

        // ── PDF ──────────────────────────────────────────────────

        public async Task<byte[]> ExportAsync(int templateId, int documentId, CancellationToken ct = default)
        {
            var template = await GetByIdAsync(templateId, ct)
                ?? throw new KeyNotFoundException($"There is no document template with ID: {templateId}");

            var data = await GetDataAsync(documentId, template.Elements, ct);

            var images = await LoadImagesAsync(template.Elements, ct);
            return _renderer.Render(template, data, images);
        }

        public async Task<byte[]> PreviewAsync(PreviewDocumentTemplateRequest request, CancellationToken ct = default)
        {
            var design = request.Template;
            design.Elements ??= [];
            EnsureFontExists(design.FontFamily);

            // Built through the entity so the preview follows exactly the same rules as saving. Nothing is stored or uploaded.
            var template = Build(design);

            DocumentTemplateData? data = null;
            if (request.DocumentId is { } documentId)
                data = await GetDataAsync(documentId, design.Elements, ct);

            // From the design, not the entity: the entity doesn't keep the data URL of a picture that isn't saved yet.
            var images = await LoadImagesAsync(design.Elements, ct);
            return _renderer.Render(ToResponse(template), data, images);
        }

        // Items always; payments and adjustments only when the template shows one of them (a second query brings both).
        // Plus the supplier/buyer details and the currency name, which DocumentResponse doesn't carry (one small query).
        private async Task<DocumentTemplateData> GetDataAsync(int documentId, List<DocumentTemplateElementDto> elements, CancellationToken ct)
        {
            var document = await _documentService.GetWithItemsAsync(documentId, false, ct)
                ?? throw new KeyNotFoundException($"There is no document with ID: {documentId}");

            if (elements.Any(e => e.Key is TemplateElementKey.PaymentsTable or TemplateElementKey.AdjustmentsTable))
            {
                var withPayments = await _documentService.GetWithPaymentsAsync(documentId, false, ct);
                document.Payments = withPayments?.Payments ?? [];
                document.Adjustments = withPayments?.Adjustments
                    .OrderBy(a => a.AdjustmentDate).ThenBy(a => a.Id).ToList() ?? [];
            }

            var extra = await _documents.Query()
    .AsNoTracking()
    .Where(d => d.Id == documentId)
    .Select(d => new
    {
        Supplier = d.Supplier == null ? null : new TemplateCompany(
            d.Supplier.Address,
            d.Supplier.Country == null ? null : d.Supplier.Country.CountryName,
            d.Supplier.RegistrationNumber,
            d.Supplier.ContactName,
            d.Supplier.Emails.OrderByDescending(e => e.IsPrimary).Select(e => e.Address).FirstOrDefault(),
            d.Supplier.Phones.OrderByDescending(p => p.IsPrimary).Select(p => p.Number).FirstOrDefault(),
            d.Supplier.BankAccounts
                .OrderByDescending(b => b.CurrencyId == d.CurrencyId)
                .ThenByDescending(b => b.IsPrimary)
                .Select(b => new TemplateBankAccount(b.BankName, b.AccountHolderName, b.IBAN, b.SwiftCode))
                .FirstOrDefault()),
        Buyer = d.Buyer == null ? null : new TemplateCompany(
            d.Buyer.Address,
            d.Buyer.Country == null ? null : d.Buyer.Country.CountryName,
            d.Buyer.RegistrationNumber,
            d.Buyer.ContactName,
            d.Buyer.Emails.OrderByDescending(e => e.IsPrimary).Select(e => e.Address).FirstOrDefault(),
            d.Buyer.Phones.OrderByDescending(p => p.IsPrimary).Select(p => p.Number).FirstOrDefault(),
            d.Buyer.BankAccounts
                .OrderByDescending(b => b.CurrencyId == d.CurrencyId)
                .ThenByDescending(b => b.IsPrimary)
                .Select(b => new TemplateBankAccount(b.BankName, b.AccountHolderName, b.IBAN, b.SwiftCode))
                .FirstOrDefault()),
        d.Currency.CurrencyName
    })
    .FirstOrDefaultAsync(ct);

            return new DocumentTemplateData
            {
                Document = document,
                Supplier = extra?.Supplier,
                Buyer = extra?.Buyer,
                CurrencyName = extra?.CurrencyName
            };
        }
        // ── Images ───────────────────────────────────────────────

        // Every image needs a new picture or one of this template's own files
        // (so a template can't point at somebody's file from another record).
        private static void EnsureImagesHaveFiles(List<DocumentTemplateElementDto> elements, IReadOnlyCollection<int> ownFileIds)
        {
            foreach (var e in elements.Where(e => e.Key == TemplateElementKey.Image && string.IsNullOrWhiteSpace(e.ImageDataUrl)))
            {
                if (e.StoredFileId is not { } fileId)
                    throw new ArgumentException("An image box has no picture. Pick one or remove the box.");

                if (!ownFileIds.Contains(fileId))
                    throw new ArgumentException($"Picture file {fileId} doesn't belong to this template.");
            }
        }

        // Uploads the new pictures (data URLs) to StoredFiles and swaps each for its file ID. Returns the new file IDs.
        private async Task<List<int>> UploadNewImagesAsync(SaveDocumentTemplateRequest request, int? templateId, CancellationToken ct)
        {
            // Decode all of them first, so a bad picture fails before anything is uploaded.
            var pending = request.Elements
                .Where(e => e.Key == TemplateElementKey.Image && !string.IsNullOrWhiteSpace(e.ImageDataUrl))
                .Select(e => (Element: e, Image: DecodeImage(e.ImageDataUrl!)))
                .ToList();

            var uploaded = new List<int>();
            foreach (var (element, image) in pending)
            {
                using var stream = new MemoryStream(image.Bytes);

                var files = await _storedFiles.UploadAsync(
                [
                    new UploadFileRequest
                    {
                        FileStream = stream,
                        FileName = $"{request.Name.Trim()} - image{image.Extension}",
                        ContentType = image.ContentType,
                        Size = image.Bytes.Length,
                        EntityType = EntityType.DocumentTemplate,
                        EntityId = templateId
                    }
                ], ct);

                element.StoredFileId = files.Single().Id;
                element.ImageDataUrl = null;
                uploaded.Add(files.Single().Id);
            }

            return uploaded;
        }

        // Picture bytes by element index, for the renderer. A missing file just leaves its box empty.
        private async Task<Dictionary<int, byte[]>> LoadImagesAsync(List<DocumentTemplateElementDto> elements, CancellationToken ct)
        {
            var images = new Dictionary<int, byte[]>();

            for (var i = 0; i < elements.Count; i++)
            {
                var e = elements[i];
                if (e.Key != TemplateElementKey.Image) continue;

                if (!string.IsNullOrWhiteSpace(e.ImageDataUrl))       // preview of a picture that isn't saved yet
                    images[i] = DecodeImage(e.ImageDataUrl).Bytes;
                else if (e.StoredFileId is { } fileId && await ReadImageFileAsync(fileId, ct) is { } bytes)
                    images[i] = bytes;
            }

            return images;
        }

        private async Task<byte[]?> ReadImageFileAsync(int fileId, CancellationToken ct)
        {
            var file = await _storedFiles.GetByIdAsync(fileId, ct);
            if (file is null || file.EntityType != EntityType.DocumentTemplate)
                return null;

            await using var stream = await _storedFiles.GetFileStreamAsync(fileId, ct);
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, ct);
            return memory.ToArray();
        }

        // "data:image/png;base64,...." → bytes. PNG and JPEG only: both StoredFiles and the PDF accept them.
        private static (byte[] Bytes, string ContentType, string Extension) DecodeImage(string dataUrl)
        {
            var comma = dataUrl.IndexOf(',');
            var header = comma > 0 ? dataUrl[..comma].Trim().ToLowerInvariant() : string.Empty;

            var (contentType, extension) = header switch
            {
                "data:image/png;base64" => ("image/png", ".png"),
                "data:image/jpeg;base64" or "data:image/jpg;base64" => ("image/jpeg", ".jpg"),
                _ => throw new ArgumentException("The picture must be a PNG or JPEG image.")
            };

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(dataUrl[(comma + 1)..]);
            }
            catch (FormatException)
            {
                throw new ArgumentException("The picture data is not valid.");
            }

            if (bytes.Length == 0)
                throw new ArgumentException("The picture is empty.");

            if (bytes.Length > DocumentTemplateRules.MaxImageBytes)
                throw new ArgumentException($"A picture must not exceed {DocumentTemplateRules.MaxImageBytes / 1024 / 1024} MB.");

            return (bytes, contentType, extension);
        }

        private async Task<List<int>> GetFileIdsAsync(int templateId, CancellationToken ct)
            => (await _storedFiles.GetByEntityIdAsync(templateId, EntityType.DocumentTemplate, ct))
                .Select(f => f.Id)
                .ToList();

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

        private static DocumentTemplate Build(SaveDocumentTemplateRequest r)
            => DocumentTemplate.Create(r.Name, r.PageSize, r.IsLandscape, r.FontFamily, ToElements(r.Elements));

        private static List<DocumentTemplateElement> ToElements(List<DocumentTemplateElementDto>? elements)
            => (elements ?? [])
                .Select(e => DocumentTemplateElement.Create(
                    e.Key, e.PageScope, e.X, e.Y, e.Width, e.Height, e.FontSize, e.Bold,
                    e.Align, e.Border, e.Color, e.Background, e.Text, e.StoredFileId,
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
                Color = e.Color,
                Background = e.Background,
                Text = e.Text,
                StoredFileId = e.StoredFileId,
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