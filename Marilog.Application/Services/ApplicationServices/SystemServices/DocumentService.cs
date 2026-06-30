using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.DTOs.Requests.DocumentDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Marilog.Kernel.Primitives;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class DocumentService : IDocumentService
    {
        private readonly IRepository<Document>      _repo;
        private readonly IRepository<SwiftTransfer> _swiftRepo;
        private readonly IRepository<Currency> _currencyRepo;
        private readonly IRepository<Payment> _paymentRepo;
        public DocumentService(IRepository<Document> repo, IRepository<SwiftTransfer> swiftRepo, IRepository<Currency> currencyRepo, IRepository<Payment> paymentRepo)
        {
            _repo      = repo;
            _swiftRepo = swiftRepo;
            _currencyRepo = currencyRepo;
            _paymentRepo = paymentRepo;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<DocumentResponse>> SearchAsync(string term, bool treeView = false, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(term))
                return [];

            var normalized = term.Trim().ToLowerInvariant();

            var tokens = normalized
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToArray();

            var docs = await _repo.Query()
                    .AsNoTracking()
                    .Where(x => x.IsActive && tokens.All(t => x.SearchVector.Contains(t)))
                    .OrderByDescending(x => x.DocDate)
                    .Take(80)
                    .Select(ToResponse())
                    .ToListAsync(ct);


            if (treeView == false)
                return docs;
            await ApplyBaseRateToTreeAsync(docs, ct);
            return BuildTree(docs, parentId: null, depth: 0);
        }
        public async Task<DocumentResponse?> GetByIdAsync(int id, bool treeView = false, CancellationToken ct = default)
        {
            var result = await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse())
                .FirstOrDefaultAsync(ct);
            if (result != null)
            {
                await ApplyBaseRateAsync(result, ct);
            }
            return result;
        }

        public async Task<DocumentResponse?> GetWithItemsAsync(int id, bool treeView = false, CancellationToken ct = default)
        {
            var result = await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponseWithItems())
                .FirstOrDefaultAsync(ct);

            if (result != null)
            {
                await ApplyBaseRateAsync(result, ct);
            }
            return result;
        }

        public async Task<DocumentResponse?> GetWithPaymentsAsync(int id, bool treeView = false, CancellationToken ct = default)
        {
            var result = await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponseWithPayments())
                .FirstOrDefaultAsync(ct);

            if (result != null)
            {
                await ApplyBaseRateAsync(result, ct);
            }
            return result;
        }
        public async Task<DocumentResponse?> GetFullAsync(int id, bool treeView = false, CancellationToken ct = default)
        {
            var result = await _repo.Query().AsNoTracking()
                          .Where(x => x.Id == id)
                          .Select(ToResponseFully())
                          .FirstOrDefaultAsync(x => x.Id == id, ct);
            if (result != null)
            {
                await ApplyBaseRateAsync(result, ct);
            }
            return result;
        } 

        public async Task<DocumentResponse?> GetByNumberAsync(string docNumber, bool treeView = false,
            CancellationToken ct = default)
        {
            var result = await _repo.Query().AsNoTracking()
                              .Where(x => x.DocNumber == docNumber)
                              .Select(ToResponse())
                              .FirstOrDefaultAsync(x => x.DocNumber == docNumber, ct);
            if (result != null)
            {
                await ApplyBaseRateAsync(result, ct);
            }
            return result;

        }

        public async Task<IReadOnlyList<DocumentResponse>> GetBySupplierAsync(int supplierId, bool treeView = false,
            CancellationToken ct = default)
        {
           
            var result =  await _repo.Query().AsNoTracking()
                          .Where(x => x.SupplierId == supplierId && x.IsActive)
                          .Select(ToResponse())
                          .OrderByDescending(x => x.DocDate)
                          .ToListAsync(ct);
            await ApplyBaseRateAsync(result, ct);

            if (treeView == false)
                return result;
            await ApplyBaseRateToTreeAsync(result, ct);
            return BuildTree(result, parentId: null, depth: 0);
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetByBuyerAsync(int buyerId, bool treeView = false,
            CancellationToken ct = default)
        {
            var result = await _repo.Query()
                          .AsNoTracking()
                          .Where(x => x.BuyerId == buyerId && x.IsActive)
                          .OrderByDescending(x => x.DocDate)
                          .Select(ToResponse())
                          .ToListAsync(ct);
            await ApplyBaseRateAsync(result, ct);
            if (treeView == false)
                return result;
            await ApplyBaseRateToTreeAsync(result, ct);
            return BuildTree(result, parentId: null, depth: 0);
        } 

        public async Task<IReadOnlyList<DocumentResponse>> GetByVesselAsync(int vesselId, bool treeView = false,
            CancellationToken ct = default)
        {
            var result = await _repo.Query().AsNoTracking()
                          .Where(x => x.VesselId == vesselId && x.IsActive)
                          .OrderByDescending(x => x.DocDate)
                          .Select(ToResponse())
                          .ToListAsync(ct);
            await ApplyBaseRateAsync(result, ct);

            if (treeView == false)
                return result;
            await ApplyBaseRateToTreeAsync(result, ct);
            return BuildTree(result, parentId: null, depth: 0);
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetByVoyageAsync(int voyageId, bool treeView = false, CancellationToken ct = default)
        {
            var result = await _repo.Query().AsNoTracking()
                                    .Where(v => v.VoyageId == voyageId)
                                    .Select(ToResponse())
                                    .ToListAsync(ct);
            await ApplyBaseRateAsync(result, ct);

            if (treeView == false)
                return result;
            await ApplyBaseRateToTreeAsync(result, ct);
            return BuildTree(result, parentId: null, depth: 0);
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetByTypeAsync(int docTypeId, bool treeView = false,
            CancellationToken ct = default)
        {
            var result = await _repo.Query().AsNoTracking()
                          .Where(x => x.DocTypeId == docTypeId && x.IsActive)
                          .OrderByDescending(x => x.DocDate)
                          .Select(ToResponse())
                          .ToListAsync(ct);
            await ApplyBaseRateAsync(result, ct);
            if (treeView == false)
                return result;
            await ApplyBaseRateToTreeAsync(result, ct);
            return BuildTree(result, parentId: null, depth: 0);
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetUnpaidAsync(bool treeView = false,
            CancellationToken ct = default)
        {
            var result = await _repo.Query().AsNoTracking()
                          .Where(x => x.IsActive && x.TotalAmount > x.Payments
                                          .Where(p => p.DocumentId == x.Id)
                                          .Sum(p => p.PaidAmount))
                          .OrderBy(x => x.DocDate)
                          .Select(ToResponse())
                          .ToListAsync(ct);
            await ApplyBaseRateAsync(result, ct);
            if(treeView == false) 
                return result;
            await ApplyBaseRateToTreeAsync(result, ct);
            return BuildTree(result, parentId: null, depth: 0);
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetChildrenAsync(int parentDocumentId,
            CancellationToken ct = default)
        {
            var result = await _repo.Query().AsNoTracking()
                          .Where(x => x.ParentDocumentId == parentDocumentId)
                          .OrderBy(x => x.DocDate)
                          .Select(ToResponse())
                          .ToListAsync(ct);
            await ApplyBaseRateAsync(result, ct);
            return result;
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetAllAsTreeAsync(CancellationToken ct = default)
        {
            // ── Query واحدة تجلب كل المستندات ────────────────────────────────────────
            var allDocs = await _repo.Query()
                .AsNoTracking()
                .OrderBy(x => x.DocDate)
                .Select(ToResponseFully())
                .ToListAsync(ct);

            await ApplyBaseRateToTreeAsync(allDocs, ct);
            return BuildTree(allDocs, parentId: null, depth: 0);
        }

        

        public async Task<DocumentResponse?> GetTreeByDocumentIdAsync(
            int documentId,
            CancellationToken ct = default)
        {
            // ── نجلب كل المستندات مرة واحدة ─────────────────────────────────────────
            var allDocs = await _repo.Query()
                .AsNoTracking()
                .OrderBy(x => x.DocDate)
                .Select(ToResponseFully())
                .ToListAsync(ct);

            await ApplyBaseRateToTreeAsync(allDocs, ct);

            // ── نبحث عن الجذر الذي ينتمي إليه هذا الـ document ─────────────────────
            var rootId = FindRootId(allDocs, documentId);
            if (rootId is null) return null;

            // ── نبني الشجرة كاملة ابتداءً من الجذر ──────────────────────────────────
            var roots = BuildTree(allDocs, parentId: null, depth: 0);
            return roots.FirstOrDefault(r => r.Id == rootId);
        }


       

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<DocumentResponse> CreateAsync(CreateDocumentRequest createDto, CancellationToken ct = default)
        {
            await EnsureUniqueDocNumberAsync(createDto.DocNumber, excludeId: null, ct);

            var document = Document.Create(
                docNumber : createDto.DocNumber,
                docTypeId  : createDto.DocTypeId,
                side : createDto.Side,
                docDate : createDto.DocDate,
                currencyId : createDto.CurrencyId,
                totalAmount : createDto.TotalAmount,
                voyageId : createDto.VoyageId,
                supplierId : createDto.SupplierId,
                buyerId : createDto.BuyerId,
                vesselId : createDto.VesselId,
                portId : createDto.PortId,
                parentDocumentId : createDto.ParentDocumentId,
                reference : createDto.Reference
                );
            await _repo.AddAsync(document, ct);
            await _repo.SaveChangesAsync(ct);
            await BuildSearchVectorAsync(document, ct);
            await _repo.SaveChangesAsync(ct);
            return new DocumentResponse
            {
                DocNumber = document.DocNumber,
                TotalAmount = document.TotalAmount,
                SupplierId = document.SupplierId,
                BuyerId = document.BuyerId,
                VesselId = document.VesselId,
                PortId = document.PortId,
                ParentDocumentId = document.ParentDocumentId,
                Reference = document.Reference,
                Side = document.Side,
                VoyageId = document.VoyageId,
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

                var document = Document.Create(c.DocNumber, c.DocTypeId, c.Side, c.DocDate, c.CurrencyId,
                                               c.TotalAmount, c.SupplierId, c.BuyerId, c.VesselId,
                                               c.PortId, c.VoyageId, c.ParentDocumentId, c.Reference);
                documents.Add(document);
            }

            await _repo.AddRangeAsync(documents, ct);
            await _repo.SaveChangesAsync(ct);

            foreach (var doc in documents)
                await BuildSearchVectorAsync(doc, ct);

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
                    Reference = doc.Reference,
                    Side = doc.Side
                })
                .ToList();
        }
        public async Task UpdateAsync(int id, UpdateDocumentRequest updateDto, CancellationToken ct = default)
        {
            var document = await GetOrThrowAsync(id, ct);
            document.Update(
                docTypeId : updateDto.DocTypeId,
                side : updateDto.Side,
                docNumber : updateDto.DocNumber,
                docDate : updateDto.DocDate,
                currencyId : updateDto.CurrencyId,
                totalAmount : updateDto.TotalAmount,
                voyageId : updateDto.VoyageId,
                parentDocumentId : updateDto.ParentDocumentId,
                supplierId : updateDto.SupplierId,
                buyerId : updateDto.BuyerId,
                vesselId : updateDto.VesselId,
                portId : updateDto.PortId,
                reference : updateDto.Reference
                );
            _repo.Update(document);
            await BuildSearchVectorAsync(document, ct);
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

            // ── Guard 1: له مدفوعات ──────────────────────────────────────────────────
            if (document.Payments.Any())
                throw new InvalidOperationException(
                    "Cannot delete a document that has payments. Deactivate it instead.");

            // ── Guard 2: له أبناء ────────────────────────────────────────────────────
            var hasChildren = await _repo.Query()
                .AnyAsync(x => x.ParentDocumentId == id, ct);

            if (hasChildren)
                throw new InvalidOperationException(
                    "Cannot delete a document that has sub-documents. " +
                    "Delete or unlink the sub-documents first.");

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

        public async Task<PaymentResponse> AddPaymentAsync(int documentId, AddPaymentRequest create, CancellationToken ct = default)
        {
            var document = await _repo.Query()
                .Include(p => p.Payments)
                .FirstOrDefaultAsync(x => x.Id == documentId, ct) ?? throw new KeyNotFoundException($"Document {documentId} not found.");

            if(create.SwiftTransferId.HasValue == true)
            {
                var swiftExists = await _swiftRepo.Query()
                .AnyAsync(x =>
                    x.Id == create.SwiftTransferId &&
                    x.IsActive &&
                    (x.SenderCompanyId == document.BuyerId ||
                     x.ReceiverCompanyId == document.SupplierId),
                    ct);

                if (!swiftExists)
                    throw new KeyNotFoundException("SwiftTransfer not found or not allowed for this document.");
            }
            

            var payment = document.AddPayment(create.SwiftTransferId, create.Method, create.PaidAmount, create.PaymentDate);

            _repo.Update(document);

            await _repo.SaveChangesAsync(ct);

            return new PaymentResponse
            {
                Id = payment.Id,
                DocumentId = payment.DocumentId,
                SwiftTransferId = payment.SwiftTransferId,
                PaidAmount = payment.PaidAmount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
            };
        }

        public async Task<PaymentResponse> UpdatePaymentAsync(int documentId, int paymentId, UpdatePaymentRequest update, CancellationToken ct = default)
        {
            var document = await _repo.Query()
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == documentId, ct)
                ?? throw new KeyNotFoundException(
                    $"Document {documentId} not found.");

            if(update.SwiftTransferId.HasValue == true)
            {
                var swiftExists = await _swiftRepo.Query()
                .AnyAsync(x => x.Id == update.SwiftTransferId && x.IsActive, ct);

                if (!swiftExists)
                    throw new KeyNotFoundException(
                        $"SwiftTransfer {update.SwiftTransferId} not found or inactive.");
            }
            

            document.UpdatePayment(paymentId, update.Method, update.SwiftTransferId, update.PaidAmount, update.PaymentDate);

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
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
            };
        }


        public async Task RemovePaymentAsync (int documentId, int  paymentId, CancellationToken ct)
        {
            var document = await GetWithPaymentsOrThrowAsync(documentId, ct);
            document.RemovePayment(paymentId);
            _repo.Update(document);
            await _repo.SaveChangesAsync(ct);
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
            var baseRate = await GetBaseCurrencyExchangeRate(ct);
            var query = _repo.Query().AsNoTracking()
                             .Include(v => v.Voyage)
                             .Include(ve => ve.Vessel)
                             .Include(p => p.Port)
                             .Where(x => x.IsActive)
                             .Where(x => x.Side != FinancialSide.None);

            // ─── فلترة ───────────────────────────────────────────────────────────
            if (options.SupplierId.HasValue)
                query = query.Where(x => x.SupplierId == options.SupplierId.Value);

            if (options.VoyageId.HasValue)
                query = query.Where(x => x.VoyageId == options.VoyageId);

            if (options.BuyerId.HasValue)
                query = query.Where(x => x.BuyerId == options.BuyerId.Value);

            if (options.VesselId.HasValue)
                query = query.Where(x => x.VesselId == options.VesselId.Value);

            if (options.DocTypeId.HasValue)
                query = query.Where(x => x.DocTypeId == options.DocTypeId.Value);

            if(options.Side.HasValue)
                query = query.Where(x => x.Side == options.Side.Value);

            if (options.UnpaidOnly == true)
                query = query.Where(x =>
                    (x.Payments.Sum(p => (decimal?)p.PaidAmount) ?? 0m) < x.TotalAmount);

            if (options.FromDate.HasValue || options.ToDate.HasValue)
            {
                if (options.FromDate.HasValue)
                    query = query.Where(x => x.DocDate >= options.FromDate.Value);

                if (options.ToDate.HasValue)
                    query = query.Where(x => x.DocDate <= options.ToDate.Value);
            }

            else if (options.LastDays.HasValue)
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
                DocNumber = x.DocNumber,
                Side = x.Side,
                VoyageId = x.VoyageId,
                VoyageNumber = x.Voyage != null ? x.Voyage.VoyageNumber : null,
                VoyageSummary = x.Voyage != null ? "From : " + x.Voyage.DeparturePort!.PortName + " To : " + x.Voyage.ArrivalPort!.PortName : null

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
                TotalAmountBase = x.TotalAmount * x.ExchangeRate / baseRate.ExchangeRate,
                PaidAmountBase = x.Paid * x.ExchangeRate / baseRate.ExchangeRate,
                RemainingBase = (x.TotalAmount - x.Paid) * x.ExchangeRate / baseRate.ExchangeRate,
                SupplierName = x.SupplierName,
                BuyerName = x.BuyerName,
                VesselName = x.VesselName,
                DocTypeName = x.DocTypeName,
                DocNumber = x.DocNumber,
                Side = x.Side,
                VoyageId = x.VoyageId,
                VoyageNumber = x.VoyageNumber,
                VoyageSummary = x.VoyageSummary,
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
                    Revenue = g.Where(d => d.Side == FinancialSide.Revenue).Sum(d => d.TotalAmountBase),
                    Expense = g.Where(d => d.Side == FinancialSide.Expense).Sum(d => d.TotalAmountBase),
                    NetPosition = g.Where(d => d.Side == FinancialSide.Revenue).Sum(d => d.TotalAmountBase)
            - g.Where(d => d.Side == FinancialSide.Expense).Sum(d => d.TotalAmountBase),
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
                    Revenue = g.Where(d => d.Side == FinancialSide.Revenue).Sum(d => d.TotalAmountBase),
                    Expense = g.Where(d => d.Side == FinancialSide.Expense).Sum(d => d.TotalAmountBase),
                    NetPosition = g.Where(d => d.Side == FinancialSide.Revenue).Sum(d => d.TotalAmountBase)
            - g.Where(d => d.Side == FinancialSide.Expense).Sum(d => d.TotalAmountBase),
                    Count = g.Count()
                })
                .OrderBy(v => v.VesselName)
                .ToList();
            // Voyage
            var voyageSummary = documents
                .Where(d => d.VoyageId.HasValue)
                .GroupBy(d => d.VoyageId!.Value)
                .Select(g => new VoyageDocumentSummary
                {
                    VoyageId = g.Key,
                    VoyageNumber = g.First().VoyageNumber ?? string.Empty,
                    VoyageSummary = g.First().VoyageSummary ?? string.Empty,
                    VesselName = g.First().VesselName ?? string.Empty,
                    TotalValue = g.Sum(d => d.TotalAmountBase),
                    TotalPaid = g.Sum(d => d.PaidAmountBase),
                    TotalRemain = g.Sum(d => d.RemainingBase),
                    Revenue = g.Where(d => d.Side == FinancialSide.Revenue).Sum(d => d.TotalAmountBase),
                    Expense = g.Where(d => d.Side == FinancialSide.Expense).Sum(d => d.TotalAmountBase),
                    NetPosition = g.Where(d => d.Side == FinancialSide.Revenue).Sum(d => d.TotalAmountBase)
                                 - g.Where(d => d.Side == FinancialSide.Expense).Sum(d => d.TotalAmountBase),
                    Count = g.Count()
                })
                .OrderBy(v => v.VoyageNumber)
                .ToList();

            var sideSummaries = documents
            .GroupBy(d => d.Side)
            .Select(g => new FinancelSideDocumentSummary
            {
                Side = g.Key.ToString(),
                Count = g.Count(),
                TotalValue = g.Sum(d => d.TotalAmountBase),
                TotalPaid = g.Sum(d => d.PaidAmountBase),
                TotalRemain = g.Sum(d => d.RemainingBase),
            })
            .ToList();

            var revenue = sideSummaries.Where(x => x.Side == FinancialSide.Revenue.ToString()).ToList();
            var expense = sideSummaries.Where(x => x.Side == FinancialSide.Expense.ToString()).ToList();
            var none = sideSummaries.Where(x => x.Side == FinancialSide.None.ToString()).ToList();
            // ─── Net Position ─────────────────────────────────────────────────────
            var netPosition = (revenue.Sum(x => x.TotalValue)) - (expense.Sum(x => x.TotalValue));
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
                RevenueSideSummary = revenue ?? [],
                ExpenseSideSummary = expense ?? [],
                VoyageSummary = voyageSummary ?? [],
                NoneSideSummary = none,
                NetPosition = netPosition,
                BaseCurrencyCode = await _currencyRepo.Query().AsNoTracking().Where(b => b.IsBaseCurrency == true).Select(bc=> bc.CurrencyCode).FirstOrDefaultAsync(ct) ?? ""
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
        private async Task<Document> GetWithPaymentsOrThrowAsync(int id, CancellationToken ct)
                   => await _repo.Query()
                                 .Include(x => x.Payments)
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

        private async Task<CurrencyResponse> GetBaseCurrencyExchangeRate(CancellationToken ct = default)
        {
            var result = await _currencyRepo.Query()
                                      .AsNoTracking()
                                      .Where(c => c.IsBaseCurrency == true)
                                      .Select(r => new CurrencyResponse
                                      {
                                          Code = r.CurrencyCode,
                                          ExchangeRate = r.ExchangeRate
                                      })
                                      .FirstOrDefaultAsync(ct);
            if (result == null)
                throw new InvalidOperationException("there is not Base Currency was set");

            return result;
        }

        private async Task ApplyBaseRateAsync(DocumentResponse document, CancellationToken ct = default)
        {
            var baseC = await GetBaseCurrencyExchangeRate(ct);

            document.TotalAmountBase /= baseC.ExchangeRate;
            document.PaidAmountBase /= baseC.ExchangeRate;
            document.RemainingBase /= baseC.ExchangeRate;
            document.CurrencyNameBase = baseC.Name;
            document.CurrencyCodeBase = baseC.Code;
        }

        private async Task ApplyBaseRateAsync(IEnumerable<DocumentResponse> documents, CancellationToken ct = default)
        {
            var baseC = await GetBaseCurrencyExchangeRate(ct);

            foreach (var document in documents)
            {
                document.TotalAmountBase /= baseC.ExchangeRate;
                document.PaidAmountBase /= baseC.ExchangeRate;
                document.RemainingBase /= baseC.ExchangeRate;
                document.CurrencyNameBase = baseC.Name;
                document.CurrencyCodeBase = baseC.Code;
            }
        }

        private async Task BuildSearchVectorAsync(Document document, CancellationToken ct)
        {
            // query واحدة تجلب كل المرتبطات دفعة واحدة
            var data = await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == document.Id)
                .Select(x => new
                {
                    SupplierName = x.Supplier != null ? x.Supplier.CompanyName : null,
                    BuyerName = x.Buyer != null ? x.Buyer.CompanyName : null,
                    VesselName = x.Vessel != null ? x.Vessel.VesselName : null,
                    CurrencyCode = x.Currency.CurrencyCode,
                    DocTypeName = x.DocType != null ? x.DocType.Name : null,
                    Port = x.Port != null ? x.Port.PortName : null,
                    Reference = x.Reference,
                    Side = x.Side.ToString(),
                })
                .FirstOrDefaultAsync(ct);

            if (data is null) return;

            document.RebuildSearchVector(
                supplierName: data.SupplierName,
                buyerName: data.BuyerName,
                vesselName: data.VesselName,
                currencyCode: data.CurrencyCode,
                totalAmount: document.TotalAmount.ToString("F2"),
                port: data.Port,
                reference: data.Reference,
                docType: data.DocTypeName,
                side : data.Side
            );
        }
        private static Expression<Func<Document, DocumentResponse>> ToResponse()
        {
            return x => new DocumentResponse
            {
                Id = x.Id,
                DocNumber = x.DocNumber,
                DocTypeId = x.DocTypeId,
                DocTypeName = x.DocType.Name,
                DocDate = x.DocDate,
                Side = x.Side,
                SupplierId = x.SupplierId,
                SupplierName = x.Supplier!.CompanyName,
                BuyerId = x.BuyerId,
                BuyerName = x.Buyer!.CompanyName,
                VesselId = x.VesselId,
                VesselName = x.Vessel != null ? x.Vessel.VesselName : null,
                PortId = x.PortId,
                PortName = x.Port != null ? x.Port.PortName : null,
                VoyageId = x.VoyageId,
                CurrencyId = x.CurrencyId,
                CurrencyCode = x.Currency.CurrencyCode,
                VoyageNumber = x.Voyage!=null ? x.Voyage.VoyageNumber : null,
                TotalAmount = x.TotalAmount,
                TotalPaid = x.Payments.Sum(p => p.PaidAmount),
                RemainingBalance = x.TotalAmount - x.Payments.Sum(p => p.PaidAmount),
                IsFullyPaid = x.TotalAmount == x.Payments.Sum(p => p.PaidAmount),

                Reference = x.Reference,
                ParentDocumentId = x.ParentDocumentId,
                IsActive = x.IsActive,

                TotalAmountBase = x.TotalAmount * x.Currency.ExchangeRate ,

                PaidAmountBase = x.Payments.Sum(p => p.PaidAmount) * x.Currency.ExchangeRate,

                RemainingBase = (x.TotalAmount - x.Payments.Sum(p => p.PaidAmount)) * x.Currency.ExchangeRate,

            };
        }

        private static Expression<Func<Document, DocumentResponse>> ToResponseWithItems()
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
                VoyageId = x.VoyageId,
                CurrencyId = x.CurrencyId,
                CurrencyCode = x.Currency.CurrencyCode,

                TotalAmount = x.TotalAmount,
                TotalPaid = x.Payments.Sum(p => p.PaidAmount),
                RemainingBalance = x.TotalAmount - x.Payments.Sum(p => p.PaidAmount),
                IsFullyPaid = x.TotalAmount == x.Payments.Sum(p => p.PaidAmount),
                Side = x.Side,
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

                TotalAmountBase = x.TotalAmount * x.Currency.ExchangeRate,

                PaidAmountBase = x.Payments.Sum(p => p.PaidAmount) * x.Currency.ExchangeRate,

                RemainingBase = (x.TotalAmount - x.Payments.Sum(p => p.PaidAmount)) * x.Currency.ExchangeRate
            };
        }

        private static Expression<Func<Document, DocumentResponse>> ToResponseWithPayments()
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
                Side = x.Side,
                CurrencyId = x.CurrencyId,
                CurrencyCode = x.Currency.CurrencyCode,
                VoyageId = x.VoyageId,
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
                    PaymentMethod = p.PaymentMethod,
                    PaymentDate = p.PaymentDate,
                    SwiftTransfer = p.SwiftTransferId == null ? null : new SwiftTransferResponse
                    {
                        AllocatedAmount = p.SwiftTransfer!.AllocatedAmount,
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
                TotalAmountBase = x.TotalAmount * x.Currency.ExchangeRate ,

                PaidAmountBase = x.Payments.Sum(p => p.PaidAmount) * x.Currency.ExchangeRate ,

                RemainingBase = (x.TotalAmount - x.Payments.Sum(p => p.PaidAmount)) * x.Currency.ExchangeRate

            };
        }

        private static Expression<Func<Document, DocumentResponse>> ToResponseFully()
        {
            return x => new DocumentResponse
            {
                Id = x.Id,
                DocNumber = x.DocNumber,
                DocTypeId = x.DocTypeId,
                DocTypeName = x.DocType.Name,
                DocDate = x.DocDate,
                Side = x.Side,
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
                VoyageId = x.VoyageId,
                Reference = x.Reference,
                ParentDocumentId = x.ParentDocumentId,
                IsActive = x.IsActive,
                Payments = x.Payments.Select(p => new PaymentResponse
                {
                    Id = p.Id,
                    PaymentMethod = p.PaymentMethod,
                    SwiftTransferId = p.SwiftTransferId,
                    DocumentId = p.DocumentId,
                    PaidAmount = p.PaidAmount,
                    PaymentDate = p.PaymentDate,
                    SwiftTransfer = p.SwiftTransferId == null ? null : new SwiftTransferResponse
                    {
                        AllocatedAmount = p.SwiftTransfer!.AllocatedAmount,
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

                TotalAmountBase = x.TotalAmount * x.Currency.ExchangeRate,

                PaidAmountBase = x.Payments.Sum(p => p.PaidAmount) * x.Currency.ExchangeRate,

                RemainingBase = (x.TotalAmount - x.Payments.Sum(p => p.PaidAmount)) * x.Currency.ExchangeRate

            };
        }

        // ── Private Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// يبني الشجرة هرمياً بشكل recursive.
        /// يأخذ كل المستندات flat ويرتبها كـ parent → children.
        /// </summary>
        private static IReadOnlyList<DocumentResponse> BuildTree(
            IReadOnlyList<DocumentResponse> allDocs,
            int? parentId,
            int depth)
        {
            var lookup = allDocs.ToLookup(x => x.ParentDocumentId);

            return BuildTreeInternal(lookup, parentId: null, depth: 0);
        }

        private static List<DocumentResponse> BuildTreeInternal(ILookup<int?, DocumentResponse> lookup,
                                                                int? parentId, int depth)
        {
            var children = lookup[parentId].ToList();

            foreach (var child in children)
            {
                child.Depth = depth;
                child.Children = BuildTreeInternal(lookup, child.Id, depth + 1);
            }

            return children;
        }

        /// <summary>
        /// يصعد من أي document للجذر عبر ParentDocumentId.
        /// يعيد Id الجذر (الذي ParentDocumentId == null).
        /// </summary>
        private static int? FindRootId(
            IReadOnlyList<DocumentResponse> allDocs,
            int documentId)
        {
            var lookup = allDocs.ToDictionary(d => d.Id);

            if (!lookup.TryGetValue(documentId, out var current))
                return null;

            // نصعد حتى نصل للجذر
            while (current.ParentDocumentId.HasValue &&
                   lookup.TryGetValue(current.ParentDocumentId.Value, out var parent))
            {
                current = parent;
            }

            return current.Id;
        }

        /// <summary>
        /// تطبيق base currency على flat list قبل بناء الشجرة.
        /// لا نحتاجها على DocumentTreeResponse حالياً لكن نتركها للتوسع.
        /// </summary>
        private async Task ApplyBaseRateToTreeAsync(IReadOnlyList<DocumentResponse> docs,
            CancellationToken ct)
        {
            if (!docs.Any()) return;

            // نجلب الـ base rate مرة واحدة فقط
            var baseC = await GetBaseCurrencyExchangeRate(ct);
            _ = baseC; // مستخدمة للتوسع لاحقاً إذا أضفت TotalAmountBase للـ TreeResponse
        }

        
    }
}
