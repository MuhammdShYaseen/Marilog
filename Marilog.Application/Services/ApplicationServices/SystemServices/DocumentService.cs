using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.DTOs.Requests.DocumentDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class DocumentService : IDocumentService
    {
        private readonly IRepository<Document>      _repo;
        private readonly IRepository<SwiftTransfer> _swiftRepo;
        private readonly IRepository<Currency> _currencyRepo;

        public DocumentService(IRepository<Document> repo, IRepository<SwiftTransfer> swiftRepo, IRepository<Currency> currencyRepo)
        {
            _repo      = repo;
            _swiftRepo = swiftRepo;
            _currencyRepo = currencyRepo;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<DocumentResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var rate = await GetBaseCurrencyExchangeRate(ct) * 1m;
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse(rate))
                .FirstOrDefaultAsync(ct);
        }

        public async Task<DocumentResponse?> GetWithItemsAsync(int id, CancellationToken ct = default)
        {
            var rate = await GetBaseCurrencyExchangeRate(ct) * 1m;
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponseWithItems(rate))
                .FirstOrDefaultAsync(ct);
        }

        public async Task<DocumentResponse?> GetWithPaymentsAsync(int id, CancellationToken ct = default)
        {
            var rate = await GetBaseCurrencyExchangeRate(ct) * 1m;
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponseWithPayments(rate))
                .FirstOrDefaultAsync(ct);
        }
        public async Task<DocumentResponse?> GetFullAsync(int id, CancellationToken ct = default)
        {
            var rate = await GetBaseCurrencyExchangeRate(ct) * 1m;
            return await _repo.Query().AsNoTracking()
                          .Where(x => x.Id == id)
                          .Select(ToResponseFully(rate))
                          .FirstOrDefaultAsync(x => x.Id == id, ct);
        } 

        public async Task<DocumentResponse?> GetByNumberAsync(string docNumber,
            CancellationToken ct = default)
        {
            var rate = await GetBaseCurrencyExchangeRate(ct) * 1m;
            return await _repo.Query().AsNoTracking()
                              .Where(x => x.DocNumber == docNumber)
                              .Select(ToResponse(rate))
                              .FirstOrDefaultAsync(x => x.DocNumber == docNumber, ct);
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetBySupplierAsync(int supplierId,
            CancellationToken ct = default)
        {
            var rate = await GetBaseCurrencyExchangeRate(ct) * 1m;
            return await _repo.Query().AsNoTracking()
                          .Where(x => x.SupplierId == supplierId && x.IsActive)
                          .Select(ToResponse(rate))
                          .OrderByDescending(x => x.DocDate)
                          .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetByBuyerAsync(int buyerId,
            CancellationToken ct = default)
        {
            var rate = await GetBaseCurrencyExchangeRate(ct) * 1m;
            return await _repo.Query()
                          .AsNoTracking()
                          .Where(x => x.BuyerId == buyerId && x.IsActive)
                          .OrderByDescending(x => x.DocDate)
                          .Select(ToResponse(rate))
                          .ToListAsync(ct);
        } 

        public async Task<IReadOnlyList<DocumentResponse>> GetByVesselAsync(int vesselId,
            CancellationToken ct = default)
        {
            var rate = await GetBaseCurrencyExchangeRate(ct) * 1m;
            return await _repo.Query().AsNoTracking()
                          .Where(x => x.VesselId == vesselId && x.IsActive)
                          .OrderByDescending(x => x.DocDate)
                          .Select(ToResponse(rate))
                          .ToListAsync(ct);

        }

        public async Task<IReadOnlyList<DocumentResponse>> GetByTypeAsync(int docTypeId,
            CancellationToken ct = default)
        {
            var rate = await GetBaseCurrencyExchangeRate(ct) * 1m;
            return await _repo.Query().AsNoTracking()
                          .Where(x => x.DocTypeId == docTypeId && x.IsActive)
                          .OrderByDescending(x => x.DocDate)
                          .Select(ToResponse(rate))
                          .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetUnpaidAsync(
            CancellationToken ct = default)
        {
            var rate = await GetBaseCurrencyExchangeRate(ct) * 1m;
            return await _repo.Query().AsNoTracking()
                          .Where(x => x.IsActive && x.TotalAmount > x.Payments
                                          .Where(p => p.DocumentId == x.Id)
                                          .Sum(p => p.PaidAmount))
                          .OrderBy(x => x.DocDate)
                          .Select(ToResponse(rate))
                          .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetChildrenAsync(int parentDocumentId,
            CancellationToken ct = default)
        {
            var rate = await GetBaseCurrencyExchangeRate(ct) * 1m;
            return await _repo.Query().AsNoTracking()
                          .Where(x => x.ParentDocumentId == parentDocumentId)
                          .OrderBy(x => x.DocDate)
                          .Select(ToResponse(rate))
                          .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<DocumentResponse> CreateAsync(string docNumber, int docTypeId, DateOnly docDate,
            int currencyId, decimal totalAmount, int? supplierId = null, int? buyerId = null,
            int? vesselId = null, int? portId = null, int? parentDocumentId = null,
            string? reference = null,
            CancellationToken ct = default)
        {
            await EnsureUniqueDocNumberAsync(docNumber, excludeId: null, ct);

            var document = Document.Create(docNumber, docTypeId, docDate, currencyId,
                                           totalAmount, supplierId, buyerId, vesselId,
                                           portId, parentDocumentId, reference);
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
                Reference = reference
            };
        }
        public async Task<IReadOnlyList<DocumentResponse>> CreateRangeAsync(
        IEnumerable<CreateDocumentRequest> commands,
        CancellationToken ct = default)
        {
            var documents = new List<Document>();

            foreach (var c in commands)
            {
                await EnsureUniqueDocNumberAsync(c.DocNumber, excludeId: null, ct);

                var document = Document.Create(c.DocNumber, c.DocTypeId, c.DocDate, c.CurrencyId,
                                               c.TotalAmount, c.SupplierId, c.BuyerId, c.VesselId,
                                               c.PortId, c.ParentDocumentId, c.Reference);
                documents.Add(document);
            }

            await _repo.AddRangeAsync(documents, ct);
            await _repo.SaveChangesAsync(ct);

            return documents
                .Select(doc => new DocumentResponse
                {
                    DocNumber = doc.DocNumber,
                    TotalAmount = doc.TotalAmount,
                    SupplierId = doc.SupplierId,
                    BuyerId = doc.BuyerId,
                    VesselId = doc.VesselId,
                    PortId = doc.PortId,
                    ParentDocumentId = doc.ParentDocumentId,
                    Reference = doc.Reference
                })
                .ToList();
        }
        public async Task UpdateAsync(int id, int docTypeId, DateOnly docDate,
            int currencyId, decimal totalAmount, int? supplierId = null, int? buyerId = null,
            int? vesselId = null, int? portId = null, int? parentDocumentId = null, string? reference = null,
            CancellationToken ct = default)
        {
            var document = await GetOrThrowAsync(id, ct);
            document.Update(docTypeId, docDate,currencyId, totalAmount, parentDocumentId, supplierId, buyerId, vesselId, portId, reference);
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

        public async Task<DocumentItemResponse> AddItemAsync(int documentId, string productName,
            decimal quantity, decimal unitPrice, string? unit = null,
            CancellationToken ct = default)
        {
            var document = await GetWithItemsOrThrowAsync(documentId, ct);
            var item = document.AddItem(productName, quantity, unitPrice, unit);
            _repo.Update(document);
            await _repo.SaveChangesAsync(ct);
            return new DocumentItemResponse
            {
                Id = item.Id,
                LineTotal = item.LineTotal,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                Unit = item.Unit

            };
        }

        public async Task<IReadOnlyList<DocumentItemResponse>> AddItemsRangeAsync(
        int documentId,
        IEnumerable<AddDocumentItemRequest> commands,
        CancellationToken ct = default)
        {
            var document = await GetWithItemsOrThrowAsync(documentId, ct);

            var items = commands
                .Select(c => document.AddItem(c.ProductName, c.Quantity, c.UnitPrice, c.Unit))
                .ToList();

            _repo.Update(document);
            await _repo.SaveChangesAsync(ct);

            return items.Select(i => new DocumentItemResponse 
            {
                Id = i.Id,
                LineTotal = i.LineTotal,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                Unit = i.Unit,
                UnitPrice = i.UnitPrice
            }).ToList();
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

        public async Task<PaymentResponse> AddPaymentAsync(int documentId, int swiftTransferId,
            decimal paidAmount, DateOnly paymentDate, CancellationToken ct = default)
        {
            var document = await _repo.Query()
                .Include(p => p.Payments)
                .FirstOrDefaultAsync(x => x.Id == documentId, ct) ?? throw new KeyNotFoundException($"Document {documentId} not found.");

            var swiftExists = await _swiftRepo.Query()
                .AnyAsync(x =>
                    x.Id == swiftTransferId &&
                    x.IsActive &&
                    (x.SenderCompanyId == document.BuyerId ||
                     x.ReceiverCompanyId == document.SupplierId),
                    ct);

            if (!swiftExists)
                throw new KeyNotFoundException("SwiftTransfer not found or not allowed for this document.");

            var payment = document.AddPayment(swiftTransferId, paidAmount, paymentDate);

            _repo.Update(document);

            await _repo.SaveChangesAsync(ct);

            return new PaymentResponse
            {
                Id = payment.Id,
                DocumentId = payment.DocumentId,
                SwiftTransferId = payment.SwiftTransferId,
                PaidAmount = payment.PaidAmount,
                PaymentDate = payment.PaymentDate
            };
        }

        public async Task<PaymentResponse> UpdatePaymentAsync(int documentId, int paymentId, int swiftTransferId, decimal paidAmount, DateOnly paymentDate, CancellationToken ct = default)
        {
            var document = await _repo.Query()
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == documentId, ct)
                ?? throw new KeyNotFoundException(
                    $"Document {documentId} not found.");

            var swiftExists = await _swiftRepo.Query()
                .AnyAsync(x => x.Id == swiftTransferId && x.IsActive, ct);

            if (!swiftExists)
                throw new KeyNotFoundException(
                    $"SwiftTransfer {swiftTransferId} not found or inactive.");

            document.UpdatePayment(paymentId, swiftTransferId, paidAmount, paymentDate);

            _repo.Update(document);

            await _repo.SaveChangesAsync(ct);

            var payment = document.Payments
                .First(x => x.Id == paymentId);

            return new PaymentResponse
            {
                Id = payment.Id,
                SwiftTransferId = payment.SwiftTransferId,
                DocumentId = payment.DocumentId,
                PaidAmount = payment.PaidAmount,
                PaymentDate = payment.PaymentDate
            };
        }

        // ── Email ──────────────────────────────────────────────────────────────────

        public async Task LogEmailAsync(int documentId, string subject, string body,
            IReadOnlyList<EmailParticipantResponse> participants, EmailDirection direction = EmailDirection.Outbound,
            CancellationToken ct = default)
        {

            var dtoParticipants = participants
                .Select(x => new Domain.Events.EmailParticipantData(
                     x.Role,
                     x.ParticipantType,
                     x.ParticipantId,
                     x.DisplayName,
                     x.EmailAddress))
                .ToList()
                .AsReadOnly();
            var document = await GetOrThrowAsync(documentId, ct);
            document.LogEmail(subject, body, dtoParticipants, direction);
            _repo.Update(document);
            await _repo.SaveChangesAsync(ct);
        }


        //----Reports----------------------------------------------------------------
        public async Task<DocumentReport> GetFilteredDocsReportAsync(
        DocumentFilterOptions options,
        CancellationToken ct = default)
        {
            var baseRate = await GetBaseCurrencyExchangeRate(ct) * 1m;
            var query = _repo.Query().AsNoTracking()
                             .Where(x => x.IsActive);

            // ─── فلترة ───────────────────────────────────────────────────────────
            if (options.SupplierId.HasValue)
                query = query.Where(x => x.SupplierId == options.SupplierId.Value);

            if (options.BuyerId.HasValue)
                query = query.Where(x => x.BuyerId == options.BuyerId.Value);

            if (options.VesselId.HasValue)
                query = query.Where(x => x.VesselId == options.VesselId.Value);

            if (options.DocTypeId.HasValue)
                query = query.Where(x => x.DocTypeId == options.DocTypeId.Value);

            if (options.UnpaidOnly == true)
                query = query.Where(x =>
                    (x.Payments.Sum(p => (decimal?)p.PaidAmount) ?? 0m) < x.TotalAmount);
            else
            {
                query = query.Where(x =>
                    (x.Payments.Sum(p => (decimal?)p.PaidAmount) ?? 0m) > x.TotalAmount);
            }



            if (options.LastDays.HasValue)
            {
                var threshold = DateTime.UtcNow.AddDays(-options.LastDays.Value);
                query = query.Where(x =>
                    x.DocDate.ToDateTime(TimeOnly.MinValue) >= threshold);
            }
            else
            {
                if (options.Year.HasValue)
                    query = query.Where(x => x.DocDate.Year == options.Year.Value);

                if (options.Month.HasValue)
                    query = query.Where(x => x.DocDate.Month == options.Month.Value);
            }

            // ─── ترتيب ───────────────────────────────────────────────────────────
            query = query.OrderByDescending(x => x.DocDate);

            // ─── الإحصاءات من DB — رحلة واحدة ───────────────────────────────────
            var dbSummary = await query
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalValue = g.Sum(x => x.TotalAmount),
                    TotalPaid = g.Sum(x => x.Payments.Sum(p => (decimal?)p.PaidAmount) ?? 0m),
                    TotalRemaining = g.Sum(x => x.TotalAmount
                                              - (x.Payments.Sum(p => (decimal?)p.PaidAmount) ?? 0m)),
                    Count = g.Count(),
                })
                .FirstOrDefaultAsync(ct);

            var docs = await query
            .Select(x => new
            {
                Paid = x.Payments.Sum(p => (decimal?)p.PaidAmount) ?? 0m,
                Id = x.Id,
                SupplierId = x.SupplierId,
                BuyerId = x.BuyerId,
                VesselId = x.VesselId,
                DocTypeId = x.DocTypeId,
                DocDate = x.DocDate,
                TotalAmount = x.TotalAmount,
                CurrencyId = x.CurrencyId,
                CurrencyCode = x.Currency.CurrencyCode,
                //CurrencySymbol = x.Currency.Symbol,
                ExchangeRate = x.Currency.ExchangeRate,         // ← أضف هذا
                IsBaseCurrency = x.Currency.IsBaseCurrency,     // ← أضف هذا
                SupplierName = x.Supplier != null ? x.Supplier.CompanyName : null,
                BuyerName = x.Buyer != null ? x.Buyer.CompanyName : null,
                VesselName = x.Vessel != null ? x.Vessel.VesselName : null,
                DocTypeName = x.DocType != null ? x.DocType.Name : null,
                DocNumber = x.DocNumber
            })
        .ToListAsync(ct);

            // ─── تحويل إلى DTO مع المبالغ بالعملة الأصلية والمبالغ بالعملة الأساسية ──
            var documents = docs.Select(x => new DocumentResponse
            {
                Id = x.Id,
                SupplierId = x.SupplierId,
                BuyerId = x.BuyerId,
                VesselId = x.VesselId,
                DocTypeId = x.DocTypeId,
                DocDate = x.DocDate,
                TotalAmount = x.TotalAmount,
                PaidAmount = x.Paid,
                Remaining = x.TotalAmount - x.Paid,
                CurrencyCode = x.CurrencyCode,
                CurrencyId = x.CurrencyId,
                //CurrencySymbol = x.CurrencySymbol,
                // المبالغ بالعملة الأساسية للمقارنة
                TotalAmountBase = x.TotalAmount * x.ExchangeRate / baseRate,
                PaidAmountBase = x.Paid * x.ExchangeRate / baseRate,
                RemainingBase = (x.TotalAmount - x.Paid) * x.ExchangeRate / baseRate,
                SupplierName = x.SupplierName,
                BuyerName = x.BuyerName,
                VesselName = x.VesselName,
                DocTypeName = x.DocTypeName,
                DocNumber = x.DocNumber
            }).ToList();

            var totalValueBase = documents.Sum(d => d.TotalAmountBase);
            var totalPaidBase = documents.Sum(d => d.PaidAmountBase);
            var totalRemainingBase = documents.Sum(d => d.RemainingBase);

            // Monthly
            var monthlySummary = documents
                .GroupBy(d => new { d.DocDate.Year, d.DocDate.Month })
                .Select(g => new MonthlyDocumentSummary
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalValue = g.Sum(d => d.TotalAmountBase),     // ← base
                    TotalPaid = g.Sum(d => d.PaidAmountBase),
                    TotalRemain = g.Sum(d => d.RemainingBase),
                    Count = g.Count()
                })
                .OrderBy(m => m.Year).ThenBy(m => m.Month)
                .ToList();

            // Supplier
            var supplierSummary = documents
                .Where(d => d.SupplierId.HasValue)
                .GroupBy(d => d.SupplierId!.Value)
                .Select(g => new SupplierDocumentSummary
                {
                    SupplierId = g.Key,
                    SupplierName = g.First().SupplierName ?? string.Empty,
                    TotalValue = g.Sum(d => d.TotalAmountBase),     // ← base
                    TotalPaid = g.Sum(d => d.PaidAmountBase),
                    TotalRemain = g.Sum(d => d.RemainingBase),
                    Count = g.Count()
                })
                .OrderBy(s => s.SupplierName)
                .ToList();

            // Vessel
            var vesselSummary = documents
                .Where(d => d.VesselId.HasValue)
                .GroupBy(d => d.VesselId!.Value)
                .Select(g => new VesselDocumentSummary
                {
                    VesselId = g.Key,
                    VesselName = g.First().VesselName ?? string.Empty,
                    TotalValue = g.Sum(d => d.TotalAmountBase),     // ← base
                    TotalPaid = g.Sum(d => d.PaidAmountBase),
                    TotalRemain = g.Sum(d => d.RemainingBase),
                    Count = g.Count()
                })
                .OrderBy(v => v.VesselName)
                .ToList();

            return new DocumentReport
            {
                Documents = documents,
                TotalValue = totalValueBase,
                TotalPaid = totalPaidBase,
                TotalRemaining = totalRemainingBase,
                Count = documents.Count,
                MonthlySummary = monthlySummary,
                SupplierSummary = supplierSummary,
                VesselSummary = vesselSummary,
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

        private async Task<decimal> GetBaseCurrencyExchangeRate(CancellationToken ct = default)
        {
            return await _currencyRepo.Query()
                                      .AsNoTracking()
                                      .Where(c => c.IsBaseCurrency == true)
                                      .Select(r => r.ExchangeRate)
                                      .FirstOrDefaultAsync(ct);
        }

        private static Expression<Func<Document, DocumentResponse>> ToResponse(decimal baseRate)
        {
            return x => new DocumentResponse
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
                ParentDocumentId = x.ParentDocumentId,
                IsActive = x.IsActive,

                TotalAmountBase = x.TotalAmount * x.Currency.ExchangeRate / baseRate,

                PaidAmountBase = x.Payments.Sum(p => p.PaidAmount) * x.Currency.ExchangeRate / baseRate,

                RemainingBase = (x.TotalAmount - x.Payments.Sum(p => p.PaidAmount)) * x.Currency.ExchangeRate / baseRate

            };
        }

        private static Expression<Func<Document, DocumentResponse>> ToResponseWithItems(decimal baseRate)
        {
            return x => new DocumentResponse
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
                }).ToList(),

                TotalItemsAmount = x.Items.Sum(i => i.LineTotal),

                Is_TotalAmount_Equal_TotalItemsAmount = x.TotalAmount == x.Items.Sum(i => i.LineTotal),

                TotalAmount_Minus_TotalItemsAmount = x.TotalAmount - x.Items.Sum(i => i.LineTotal),

                TotalAmountBase = x.TotalAmount * x.Currency.ExchangeRate / baseRate,

                PaidAmountBase = x.Payments.Sum(p => p.PaidAmount) * x.Currency.ExchangeRate / baseRate,

                RemainingBase = (x.TotalAmount - x.Payments.Sum(p => p.PaidAmount)) * x.Currency.ExchangeRate / baseRate
            };
        }

        private static Expression<Func<Document, DocumentResponse>> ToResponseWithPayments(decimal baseRate)
        {
            return x => new DocumentResponse
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

                }).ToList(),
                TotalAmountBase = x.TotalAmount * x.Currency.ExchangeRate / baseRate,

                PaidAmountBase = x.Payments.Sum(p => p.PaidAmount) * x.Currency.ExchangeRate / baseRate,

                RemainingBase = (x.TotalAmount - x.Payments.Sum(p => p.PaidAmount)) * x.Currency.ExchangeRate / baseRate

            };
        }

        private static Expression<Func<Document, DocumentResponse>> ToResponseFully(decimal baseRate)
        {
            return x => new DocumentResponse
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
                }).ToList(),
                TotalItemsAmount = x.Items.Sum(i => i.LineTotal),

                Is_TotalAmount_Equal_TotalItemsAmount = x.TotalAmount == x.Items.Sum(i => i.LineTotal),

                TotalAmount_Minus_TotalItemsAmount = x.TotalAmount - x.Items.Sum(i => i.LineTotal),

                TotalAmountBase = x.TotalAmount * x.Currency.ExchangeRate / baseRate,

                PaidAmountBase = x.Payments.Sum(p => p.PaidAmount) * x.Currency.ExchangeRate / baseRate,

                RemainingBase = (x.TotalAmount - x.Payments.Sum(p => p.PaidAmount)) * x.Currency.ExchangeRate / baseRate

            };
        }
    }
}
