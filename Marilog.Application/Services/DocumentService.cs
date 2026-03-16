using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.Interfaces.Services;
using Marilog.Domain.Events;

namespace Marilog.Application.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IRepository<Document>      _repo;
        private readonly IRepository<SwiftTransfer> _swiftRepo;

        public DocumentService(
            IRepository<Document>      repo,
            IRepository<SwiftTransfer> swiftRepo)
        {
            _repo      = repo;
            _swiftRepo = swiftRepo;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<Document?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.Query()
                          .Include(x => x.DocType)
                          .Include(x => x.Currency)
                          .Include(x => x.Supplier)
                          .Include(x => x.Buyer)
                          .Include(x => x.Vessel)
                          .Include(x => x.Port)
                          .FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<Document?> GetWithItemsAsync(int id, CancellationToken ct = default)
            => await _repo.Query()
                          .Include(x => x.Items)
                          .Include(x => x.DocType)
                          .Include(x => x.Currency)
                          .FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<Document?> GetWithPaymentsAsync(int id, CancellationToken ct = default)
            => await _repo.Query()
                          .Include(x => x.Payments)
                          .Include(x => x.DocType)
                          .Include(x => x.Currency)
                          .FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<Document?> GetFullAsync(int id, CancellationToken ct = default)
            => await _repo.Query()
                          .Include(x => x.Items)
                          .Include(x => x.Payments)
                          .Include(x => x.DocType)
                          .Include(x => x.Currency)
                          .Include(x => x.Supplier)
                          .Include(x => x.Buyer)
                          .Include(x => x.Vessel)
                          .Include(x => x.Port)
                          .FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<Document?> GetByNumberAsync(string docNumber,
            CancellationToken ct = default)
            => await _repo.Query()
                          .Include(x => x.DocType)
                          .Include(x => x.Currency)
                          .FirstOrDefaultAsync(x => x.DocNumber == docNumber, ct);

        public async Task<IReadOnlyList<Document>> GetBySupplierAsync(int supplierId,
            CancellationToken ct = default)
            => await _repo.Query()
                          .Where(x => x.SupplierId == supplierId && x.IsActive)
                          .Include(x => x.DocType)
                          .Include(x => x.Currency)
                          .OrderByDescending(x => x.DocDate)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Document>> GetByBuyerAsync(int buyerId,
            CancellationToken ct = default)
            => await _repo.Query()
                          .Where(x => x.BuyerId == buyerId && x.IsActive)
                          .Include(x => x.DocType)
                          .Include(x => x.Currency)
                          .OrderByDescending(x => x.DocDate)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Document>> GetByVesselAsync(int vesselId,
            CancellationToken ct = default)
            => await _repo.Query()
                          .Where(x => x.VesselId == vesselId && x.IsActive)
                          .Include(x => x.DocType)
                          .Include(x => x.Currency)
                          .OrderByDescending(x => x.DocDate)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Document>> GetByTypeAsync(int docTypeId,
            CancellationToken ct = default)
            => await _repo.Query()
                          .Where(x => x.DocTypeId == docTypeId && x.IsActive)
                          .Include(x => x.Currency)
                          .OrderByDescending(x => x.DocDate)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Document>> GetUnpaidAsync(
            CancellationToken ct = default)
            => await _repo.Query()
                          .Where(x => x.IsActive &&
                                      x.TotalAmount > x.Payments
                                          .Where(p => p.DocumentId == x.Id)
                                          .Sum(p => p.PaidAmount))
                          .Include(x => x.DocType)
                          .Include(x => x.Currency)
                          .Include(x => x.Supplier)
                          .OrderBy(x => x.DocDate)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Document>> GetChildrenAsync(int parentDocumentId,
            CancellationToken ct = default)
            => await _repo.Query()
                          .Where(x => x.ParentDocumentId == parentDocumentId)
                          .Include(x => x.DocType)
                          .Include(x => x.Currency)
                          .OrderBy(x => x.DocDate)
                          .ToListAsync(ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<Document> CreateAsync(string docNumber, int docTypeId, DateOnly docDate,
            int currencyId, decimal totalAmount, int? supplierId = null, int? buyerId = null,
            int? vesselId = null, int? portId = null, int? parentDocumentId = null,
            string? reference = null, string? filePath = null,
            CancellationToken ct = default)
        {
            await EnsureUniqueDocNumberAsync(docNumber, excludeId: null, ct);

            var document = Document.Create(docNumber, docTypeId, docDate, currencyId,
                                           totalAmount, supplierId, buyerId, vesselId,
                                           portId, parentDocumentId, reference, filePath);
            await _repo.AddAsync(document, ct);
            await _repo.SaveChangesAsync(ct);
            return document;
        }

        public async Task UpdateAsync(int id, int docTypeId, DateOnly docDate,
            int currencyId, decimal totalAmount, int? supplierId = null, int? buyerId = null,
            int? vesselId = null, int? portId = null, string? reference = null,
            string? filePath = null, CancellationToken ct = default)
        {
            var document = await GetOrThrowAsync(id, ct);
            document.Update(docTypeId, docDate, currencyId, totalAmount, supplierId,
                            buyerId, vesselId, portId, reference, filePath);
            _repo.Update(document);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task LinkToParentAsync(int id, int parentDocumentId,
            CancellationToken ct = default)
        {
            var document = await GetOrThrowAsync(id, ct);
            var parentExists = await _repo.Query()
                .AnyAsync(x => x.Id == parentDocumentId, ct);
            if (!parentExists)
                throw new KeyNotFoundException($"Parent document {parentDocumentId} not found.");

            document.LinkToParent(parentDocumentId);
            _repo.Update(document);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task UnlinkFromParentAsync(int id, CancellationToken ct = default)
        {
            var document = await GetOrThrowAsync(id, ct);
            document.UnlinkFromParent();
            _repo.Update(document);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var document = await GetOrThrowAsync(id, ct);
            document.Activate();
            _repo.Update(document);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var document = await GetOrThrowAsync(id, ct);
            document.Deactivate();
            _repo.Update(document);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var document = await _repo.Query()
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new KeyNotFoundException($"Document {id} not found.");

            if (document.Payments.Any())
                throw new InvalidOperationException(
                    "Cannot delete a document that has payments. Deactivate it instead.");

            _repo.HardDelete(document);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Items ─────────────────────────────────────────────────────────────────

        public async Task<DocumentItem> AddItemAsync(int documentId, string productName,
            decimal quantity, decimal unitPrice, string? unit = null,
            CancellationToken ct = default)
        {
            var document = await GetWithItemsOrThrowAsync(documentId, ct);
            var item = document.AddItem(productName, quantity, unitPrice, unit);
            _repo.Update(document);
            await _repo.SaveChangesAsync(ct);
            return item;
        }

        public async Task UpdateItemAsync(int documentId, int itemId, string productName,
            decimal quantity, decimal unitPrice, string? unit = null,
            CancellationToken ct = default)
        {
            var document = await GetWithItemsOrThrowAsync(documentId, ct);
            document.UpdateItem(itemId, productName, quantity, unitPrice, unit);
            _repo.Update(document);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task RemoveItemAsync(int documentId, int itemId,
            CancellationToken ct = default)
        {
            var document = await GetWithItemsOrThrowAsync(documentId, ct);
            document.RemoveItem(itemId);
            _repo.Update(document);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Payments ──────────────────────────────────────────────────────────────

        public async Task<Payment> AddPaymentAsync(int documentId, int swiftTransferId,
            decimal paidAmount, DateOnly paymentDate, CancellationToken ct = default)
        {
            var document = await _repo.Query()
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == documentId, ct)
                ?? throw new KeyNotFoundException($"Document {documentId} not found.");

            var swiftExists = await _swiftRepo.Query()
                .AnyAsync(x => x.Id == swiftTransferId && x.IsActive, ct);
            if (!swiftExists)
                throw new KeyNotFoundException($"SwiftTransfer {swiftTransferId} not found or inactive.");

            var payment = document.AddPayment(swiftTransferId, paidAmount, paymentDate);
            _repo.Update(document);
            await _repo.SaveChangesAsync(ct);
            return payment;
        }

        // ── Email ──────────────────────────────────────────────────────────────────

        public async Task LogEmailAsync(int documentId, string subject, string body,
            IReadOnlyList<EmailParticipantData> participants,
            EmailDirection direction = EmailDirection.Outbound,
            CancellationToken ct = default)
        {
            var document = await GetOrThrowAsync(documentId, ct);
            document.LogEmail(subject, body, participants, direction);
            _repo.Update(document);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<Document> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"Document {id} not found.");

        private async Task<Document> GetWithItemsOrThrowAsync(int id, CancellationToken ct)
            => await _repo.Query()
                          .Include(x => x.Items)
                          .FirstOrDefaultAsync(x => x.Id == id, ct)
               ?? throw new KeyNotFoundException($"Document {id} not found.");

        private async Task EnsureUniqueDocNumberAsync(string docNumber,
            int? excludeId, CancellationToken ct)
        {
            var conflict = await _repo.Query()
                .AnyAsync(x => x.DocNumber == docNumber &&
                               (excludeId == null || x.Id != excludeId), ct);
            if (conflict)
                throw new InvalidOperationException(
                    $"Document number '{docNumber}' already exists.");
        }
    }
}
