using Marilog.Application.DTOs;
using Marilog.Application.DTOs.Reports.DocumentReports;
using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities;
using Marilog.Domain.Events;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        public async Task<DocumentResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<DocumentResponse?> GetWithItemsAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponseWithItems)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<DocumentResponse?> GetWithPaymentsAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponseWithPayments)
                .FirstOrDefaultAsync(ct);
        }
        public async Task<DocumentResponse?> GetFullAsync(int id, CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where (x => x.Id == id)
                          .Select(ToResponseFully)
                          .FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<DocumentResponse?> GetByNumberAsync(string docNumber,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.DocNumber == docNumber)
                          .Select(ToResponse)
                          .FirstOrDefaultAsync(x => x.DocNumber == docNumber, ct);

        public async Task<IReadOnlyList<DocumentResponse>> GetBySupplierAsync(int supplierId,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.SupplierId == supplierId && x.IsActive)
                          .Select (ToResponse)
                          .OrderByDescending(x => x.DocDate)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<DocumentResponse>> GetByBuyerAsync(int buyerId,
            CancellationToken ct = default)
            => await _repo.Query()
                          .AsNoTracking()
                          .Where(x => x.BuyerId == buyerId && x.IsActive)
                          .OrderByDescending(x => x.DocDate)
                          .Select(ToResponse)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<DocumentResponse>> GetByVesselAsync(int vesselId,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.VesselId == vesselId && x.IsActive)
                          .OrderByDescending(x => x.DocDate)
                          .Select(ToResponse)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<DocumentResponse>> GetByTypeAsync(int docTypeId,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.DocTypeId == docTypeId && x.IsActive)
                          .OrderByDescending(x => x.DocDate)
                          .Select(ToResponse)
                          .ToListAsync(ct);


        public async Task<IReadOnlyList<DocumentResponse>> GetUnpaidAsync(
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.IsActive &&
                                      x.TotalAmount > x.Payments
                                          .Where(p => p.DocumentId == x.Id)
                                          .Sum(p => p.PaidAmount))
                          .OrderBy(x => x.DocDate)
                          .Select(ToResponse)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<DocumentResponse>> GetChildrenAsync(int parentDocumentId,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.ParentDocumentId == parentDocumentId)
                          .OrderBy(x => x.DocDate)
                          .Select(ToResponse)
                          .ToListAsync(ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<DocumentResponse> CreateAsync(string docNumber, int docTypeId, DateOnly docDate,
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
            return new DocumentResponse
            {
                DocNumber = docNumber,
                TotalAmount = totalAmount,
                SupplierId = supplierId,
                BuyerId = buyerId,
                VesselId = vesselId,
                PortId = portId,
                ParentDocumentId = parentDocumentId,
                Reference = reference,
                FilePath = filePath
            };
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
            var parentExists = await _repo.Query().AsNoTracking()
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

        public async Task RemoveItemAsync(int documentId, int itemId, CancellationToken ct = default)
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


        //----Reports----------------------------------------------------------------
        public async Task<DocumentReport> GetFilteredDocsReportAsync(DocumentFilterOptions options, CancellationToken ct = default)
        {
            var query = _repo.Query().AsNoTracking()
                             .Where(x => x.IsActive); // قاعدة كل الفلاتر

            // 1. فلترة حسب المورد
            if (options.SupplierId.HasValue)
                query = query.Where(x => x.SupplierId == options.SupplierId.Value);

            // 2. فلترة حسب المشتري
            if (options.BuyerId.HasValue)
                query = query.Where(x => x.BuyerId == options.BuyerId.Value);

            // 3. فلترة حسب السفينة
            if (options.VesselId.HasValue)
                query = query.Where(x => x.VesselId == options.VesselId.Value);

            // 4. فلترة حسب نوع المستند
            if (options.DocTypeId.HasValue)
                query = query.Where(x => x.DocTypeId == options.DocTypeId.Value);

            // 5. المستندات الغير مدفوعة فقط
            if (options.UnpaidOnly.HasValue && options.UnpaidOnly.Value)
            {
                query = query.Where(x => x.TotalAmount > x.Payments
                                                .Where(p => p.DocumentId == x.Id)
                                                .Sum(p => p.PaidAmount));
            }

            // 6. فلترة حسب آخر X أيام
            if (options.LastDays.HasValue)
            {
                var thresholdDate = DateTime.UtcNow.AddDays(-options.LastDays.Value);
                query = query.Where(x => x.DocDate.ToDateTime(TimeOnly.MinValue) >= thresholdDate);
            }

            // 7. فلترة حسب السنة
            if (options.Year.HasValue)
                query = query.Where(x => x.DocDate.Year == options.Year.Value);

            // 8. فلترة حسب الشهر
            if (options.Month.HasValue)
                query = query.Where(x => x.DocDate.Month == options.Month.Value);

            // 9. ترتيب افتراضي حسب التاريخ تنازلي
            query = query.OrderByDescending(x => x.DocDate);

            // 10. جلب البيانات وتحويلها إلى DTO
            var docs = await query.Select(ToResponse).ToListAsync(ct);

            // 11. العمليات التحليلية
            var totalValue = docs.Sum(d => d.TotalAmount);
            var count = docs.Count;

            var monthlyTotals = docs
                .GroupBy(d => new { d.DocDate.Year, d.DocDate.Month })
                .Select(g => new MonthlyTotal
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalValue = g.Sum(x => x.TotalAmount),
                    Count = g.Count()
                })
                .OrderBy(mt => mt.Year).ThenBy(mt => mt.Month)
                .ToList();

            var yearlyTotals = docs
                .GroupBy(d => d.DocDate.Year)
                .Select(g => new YearlyTotal
                {
                    Year = g.Key,
                    TotalValue = g.Sum(x => x.TotalAmount),
                    Count = g.Count()
                })
                .OrderBy(yt => yt.Year)
                .ToList();

            return new DocumentReport
            {
                Documents = docs,
                TotalValue = totalValue,
                Count = count,
                MonthlyTotals = monthlyTotals,
                YearlyTotals = yearlyTotals
            };
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

        private static readonly Expression<Func<Document, DocumentResponse>> ToResponse =
        x => new DocumentResponse
        {
            Id = x.Id,
            DocNumber = x.DocNumber,
            DocTypeId = x.DocTypeId,
            DocTypeName = x.DocType.Name,
            DocDate = x.DocDate,

            SupplierId = x.SupplierId,
            SupplierName = x.Supplier!.CompanyName,
            BuyerId = x.BuyerId,
            BuyerName = x.Buyer!.CompanyName,
            VesselId = x.VesselId,
            VesselName = x.Vessel != null ? x.Vessel.VesselName : null,
            PortId = x.PortId,
            PortName = x.Port != null ? x.Port.PortName : null,

            CurrencyId = x.CurrencyId,
            CurrencyCode = x.Currency.CurrencyCode,

            TotalAmount = x.TotalAmount,
            TotalPaid = x.Payments.Sum(p => p.PaidAmount),
            RemainingBalance = x.TotalAmount - x.Payments.Sum(p => p.PaidAmount),
            IsFullyPaid = x.TotalAmount == x.Payments.Sum(p => p.PaidAmount),

            Reference = x.Reference,
            FilePath = x.FilePath,
            ParentDocumentId = x.ParentDocumentId,
            IsActive = x.IsActive
        };

        private static readonly Expression<Func<Document, DocumentResponse>> ToResponseWithItems =
        x => new DocumentResponse
        {
            Id = x.Id,
            DocNumber = x.DocNumber,
            DocTypeId = x.DocTypeId,
            DocTypeName = x.DocType.Name,
            DocDate = x.DocDate,

            SupplierId = x.SupplierId,
            SupplierName = x.Supplier!.CompanyName,
            BuyerId = x.BuyerId,
            BuyerName = x.Buyer!.CompanyName,
            VesselId = x.VesselId,
            VesselName = x.Vessel != null ? x.Vessel.VesselName : null,
            PortId = x.PortId,
            PortName = x.Port != null ? x.Port.PortName : null,

            CurrencyId = x.CurrencyId,
            CurrencyCode = x.Currency.CurrencyCode,

            TotalAmount = x.TotalAmount,
            TotalPaid = x.Payments.Sum(p => p.PaidAmount),
            RemainingBalance = x.TotalAmount - x.Payments.Sum(p => p.PaidAmount),
            IsFullyPaid = x.TotalAmount == x.Payments.Sum(p => p.PaidAmount),

            Reference = x.Reference,
            FilePath = x.FilePath,
            ParentDocumentId = x.ParentDocumentId,
            IsActive = x.IsActive,

            Items = x.Items.Select(i => new DocumentItemResponse
            {
                Id = i.Id,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.LineTotal,
                Unit = i.Unit,
            }).ToList()
        };

        private static readonly Expression<Func<Document, DocumentResponse>> ToResponseWithPayments =
        x => new DocumentResponse
        {
            Id = x.Id,
            DocNumber = x.DocNumber,
            DocTypeId = x.DocTypeId,
            DocTypeName = x.DocType.Name,
            DocDate = x.DocDate,

            SupplierId = x.SupplierId,
            SupplierName = x.Supplier!.CompanyName,
            BuyerId = x.BuyerId,
            BuyerName = x.Buyer!.CompanyName,
            VesselId = x.VesselId,
            VesselName = x.Vessel != null ? x.Vessel.VesselName : null,
            PortId = x.PortId,
            PortName = x.Port != null ? x.Port.PortName : null,

            CurrencyId = x.CurrencyId,
            CurrencyCode = x.Currency.CurrencyCode,

            TotalAmount = x.TotalAmount,
            TotalPaid = x.Payments.Sum(p => p.PaidAmount),
            RemainingBalance = x.TotalAmount - x.Payments.Sum(p => p.PaidAmount),
            IsFullyPaid = x.TotalAmount == x.Payments.Sum(p => p.PaidAmount),

            Reference = x.Reference,
            FilePath = x.FilePath,
            ParentDocumentId = x.ParentDocumentId,
            IsActive = x.IsActive,
            Payments = x.Payments.Select(p => new PaymentResponse
            {
                Id = p.Id,
                SwiftTransferId = p.SwiftTransferId,
                DocumentId = p.DocumentId,
                PaidAmount = p.PaidAmount,
                PaymentDate = p.PaymentDate,
                SwiftTransfer = new SwiftTransferResponse
                {
                    AllocatedAmount = p.SwiftTransfer.AllocatedAmount,
                    SenderBank = p.SwiftTransfer.SenderBank,
                    SenderCompanyId = p.SwiftTransfer.SenderCompanyId,
                    SenderCompanyName = p.SwiftTransfer.SenderCompany!.CompanyName,
                    SwiftReference = p.SwiftTransfer.SwiftReference,
                    Amount = p.SwiftTransfer.Amount,
                    CurrencyCode = p.SwiftTransfer.Currency.CurrencyCode,
                    Id = p.SwiftTransferId,
                    CurrencyId = p.SwiftTransfer.CurrencyId,
                    IsActive = p.SwiftTransfer.IsActive,
                    IsFullyAllocated = p.SwiftTransfer.IsFullyAllocated,
                    PaymentReference = p.SwiftTransfer.PaymentReference,
                    ReceiverBank = p.SwiftTransfer.ReceiverBank,
                    ReceiverCompanyId = p.SwiftTransfer.ReceiverCompanyId,
                    ReceiverCompanyName = p.SwiftTransfer.ReceiverCompany!.CompanyName,
                    TransactionDate = p.SwiftTransfer.TransactionDate,
                    UnallocatedAmount = p.SwiftTransfer.UnallocatedAmount
                }

            }).ToList()
            
        };

        private static readonly Expression<Func<Document, DocumentResponse>> ToResponseFully =
        x => new DocumentResponse
        {
           Id = x.Id,
           DocNumber = x.DocNumber,
           DocTypeId = x.DocTypeId,
           DocTypeName = x.DocType.Name,
           DocDate = x.DocDate,

           SupplierId = x.SupplierId,
           SupplierName = x.Supplier!.CompanyName,
           BuyerId = x.BuyerId,
           BuyerName = x.Buyer!.CompanyName,
           VesselId = x.VesselId,
           VesselName = x.Vessel != null ? x.Vessel.VesselName : null,
           PortId = x.PortId,
           PortName = x.Port != null ? x.Port.PortName : null,

           CurrencyId = x.CurrencyId,
           CurrencyCode = x.Currency.CurrencyCode,

           TotalAmount = x.TotalAmount,
           TotalPaid = x.Payments.Sum(p => p.PaidAmount),
           RemainingBalance = x.TotalAmount - x.Payments.Sum(p => p.PaidAmount),
           IsFullyPaid = x.TotalAmount == x.Payments.Sum(p => p.PaidAmount),

           Reference = x.Reference,
           FilePath = x.FilePath,
           ParentDocumentId = x.ParentDocumentId,
           IsActive = x.IsActive,
           Payments = x.Payments.Select(p => new PaymentResponse
           {
               Id = p.Id,
               SwiftTransferId = p.SwiftTransferId,
               DocumentId = p.DocumentId,
               PaidAmount = p.PaidAmount,
               PaymentDate = p.PaymentDate,
               SwiftTransfer = new SwiftTransferResponse
               {
                   AllocatedAmount = p.SwiftTransfer.AllocatedAmount,
                   SenderBank = p.SwiftTransfer.SenderBank,
                   SenderCompanyId = p.SwiftTransfer.SenderCompanyId,
                   SenderCompanyName = p.SwiftTransfer.SenderCompany!.CompanyName,
                   SwiftReference = p.SwiftTransfer.SwiftReference,
                   Amount = p.SwiftTransfer.Amount,
                   CurrencyCode = p.SwiftTransfer.Currency.CurrencyCode,
                   Id = p.SwiftTransferId,
                   CurrencyId = p.SwiftTransfer.CurrencyId,
                   IsActive = p.SwiftTransfer.IsActive,
                   IsFullyAllocated = p.SwiftTransfer.IsFullyAllocated,
                   PaymentReference = p.SwiftTransfer.PaymentReference,
                   ReceiverBank = p.SwiftTransfer.ReceiverBank,
                   ReceiverCompanyId = p.SwiftTransfer.ReceiverCompanyId,
                   ReceiverCompanyName = p.SwiftTransfer.ReceiverCompany!.CompanyName,
                   TransactionDate = p.SwiftTransfer.TransactionDate,
                   UnallocatedAmount = p.SwiftTransfer.UnallocatedAmount
               },

           }).ToList(),
            Items = x.Items.Select(i => new DocumentItemResponse
            {
                Id = i.Id,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.LineTotal,
                Unit = i.Unit,
            }).ToList()

        };
    }
}
