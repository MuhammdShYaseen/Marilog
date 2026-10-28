using Marilog.Contracts.DTOs.Reports.PaymentReports;
using Marilog.Contracts.DTOs.Requests.DocumentDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class PaymentService : IPaymentService
    {

        private readonly IRepository<Document> _repo;
        private readonly IRepository<SwiftTransfer> _swiftRepo;
        private readonly IRepository<Currency> _currencyRepo;
        public PaymentService(IRepository<Document> documentRepo, IRepository<SwiftTransfer> swiftRepo, IRepository<Currency> currencyRepo) 
        {
            _repo = documentRepo;
            _swiftRepo = swiftRepo;
            _currencyRepo = currencyRepo;
        }
        public async Task<PaymentResponse> AddPaymentAsync(int documentId, AddPaymentRequest create, CancellationToken ct = default)
        {
            var document = await _repo.Query()
                .Include(p => p.Payments)
                .Include(a => a.Adjustments)
                .FirstOrDefaultAsync(x => x.Id == documentId, ct)
                ?? throw new KeyNotFoundException($"Document {documentId} not found.");

            if (create.SwiftTransferId.HasValue)
            {
                var swift = await _swiftRepo.Query()
                    .Include(p => p.Payments)
                    .Where(x =>
                        x.Id == create.SwiftTransferId &&
                        x.IsActive &&
                        (x.SenderCompanyId == document.BuyerId ||
                         x.ReceiverCompanyId == document.SupplierId))
                    .Select(x => new { x.CurrencyId, x.UnallocatedAmount, x.Status })
                    .FirstOrDefaultAsync(ct);

                if (swift is null)
                    throw new KeyNotFoundException(
                        "SwiftTransfer not found or not allowed for this document.");

                if (swift.Status != SwiftTransferStatus.Received)
                    throw new InvalidOperationException(
                        "Cannot allocate a payment to a cancelled or unreceived SwiftTransfer.");

                if (swift.CurrencyId != document.CurrencyId)
                    throw new InvalidOperationException(
                        "SwiftTransfer currency does not match the document currency.");

                if (create.PaidAmount > swift.UnallocatedAmount)
                    throw new InvalidOperationException(
                        $"Paid amount ({create.PaidAmount}) exceeds the unallocated SwiftTransfer amount ({swift.UnallocatedAmount}).");
            }

            var payment = document.AddPayment(
                create.SwiftTransferId,
                create.Method,
                create.PaidAmount,
                create.PaymentDate,
                create.Discreption);

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
                Discreption = payment.Discreption,
            };
        }

        public async Task<PaymentResponse> UpdatePaymentAsync(
            int documentId, int paymentId, UpdatePaymentRequest update,
            CancellationToken ct = default)
        {
            var document = await _repo.Query()
                .Include(x => x.Payments)
                .Include(x => x.Adjustments)
                .FirstOrDefaultAsync(x => x.Id == documentId, ct)
                ?? throw new KeyNotFoundException($"Document {documentId} not found.");

            var existingPayment = document.Payments.FirstOrDefault(x => x.Id == paymentId)
                ?? throw new KeyNotFoundException($"Payment {paymentId} not found.");

            if (update.SwiftTransferId.HasValue)
            {
                var swift = await _swiftRepo.Query()
                    .Include(p => p.Payments)
                    .Where(x => x.Id == update.SwiftTransferId && x.IsActive)
                    .Select(x => new { x.CurrencyId, x.UnallocatedAmount, x.Status })
                    .FirstOrDefaultAsync(ct);

                if (swift is null)
                    throw new KeyNotFoundException(
                        $"SwiftTransfer {update.SwiftTransferId} not found or inactive.");

                if (swift.Status != SwiftTransferStatus.Received)
                    throw new InvalidOperationException(
                        "Cannot allocate a payment to a cancelled or unreceived SwiftTransfer.");

                if (swift.CurrencyId != document.CurrencyId)
                    throw new InvalidOperationException(
                        "SwiftTransfer currency does not match the document currency.");

                // إذا نفس الـ SwiftTransfer القديم، لازم نرجع المبلغ القديم قبل ما نقارن
                var alreadyAllocated = existingPayment.SwiftTransferId == update.SwiftTransferId
                    ? existingPayment.PaidAmount
                    : 0;

                var availableAmount = swift.UnallocatedAmount + alreadyAllocated;

                if (update.PaidAmount > availableAmount)
                    throw new InvalidOperationException(
                        $"Paid amount ({update.PaidAmount}) exceeds the unallocated SwiftTransfer amount ({availableAmount}).");
            }

            document.UpdatePayment(
                paymentId,
                update.Method,
                update.SwiftTransferId,
                update.PaidAmount,
                update.PaymentDate,
                update.Discreption);

            _repo.Update(document);
            await _repo.SaveChangesAsync(ct);

            var payment = document.Payments.First(x => x.Id == paymentId);

            return new PaymentResponse
            {
                Id = payment.Id,
                DocumentId = payment.DocumentId,
                SwiftTransferId = payment.SwiftTransferId,
                PaidAmount = payment.PaidAmount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                Discreption = payment.Discreption,
            };
        }

        public async Task RemovePaymentAsync(int documentId, int paymentId, CancellationToken ct)
        {
            var document = await GetWithPaymentsOrThrowAsync(documentId, ct);
            document.RemovePayment(paymentId);
            _repo.Update(document);
            await _repo.SaveChangesAsync(ct);
        }



        // ══ Reports ═══════════════════════════════════════════════════════════════

        public async Task<PaymentsReport> GetPaymentsReportAsync(
            FilterPaymentOptionsRequest options, CancellationToken ct = default)
        {
            var baseRate = await GetBaseCurrencyExchangeRate(ct);

            var documentsQuery = _repo.Query()
                .AsNoTracking()
                .Where(p => p.IsActive && p.Side != FinancialSide.None);

            // ─── فلترة الكيانات (على مستوى المستند) ──────────────────────────────
            if (options.VesselId.HasValue)
                documentsQuery = documentsQuery.Where(p => p.VesselId == options.VesselId.Value);

            if (options.SupplierId.HasValue)
                documentsQuery = documentsQuery.Where(p => p.SupplierId == options.SupplierId.Value);

            if (options.BuyerId.HasValue)
                documentsQuery = documentsQuery.Where(p => p.BuyerId == options.BuyerId.Value);

            if (options.VoyageId.HasValue)
                documentsQuery = documentsQuery.Where(p => p.VoyageId == options.VoyageId.Value);

            if (options.DocTypeId.HasValue)
                documentsQuery = documentsQuery.Where(p => p.DocTypeId == options.DocTypeId.Value);

            if (options.Side.HasValue)
                documentsQuery = documentsQuery.Where(p => p.Side == options.Side.Value);

            // ─── النزول لمستوى الدفعة الفعلي — هون المفتاح ───────────────────────
            var paymentsQuery = documentsQuery.SelectMany(
                d => d.Payments,
                (d, payment) => new { Document = d, Payment = payment });

            // ─── فلترة على مستوى الدفعة نفسها ────────────────────────────────────
            if (options.FromDate.HasValue)
                paymentsQuery = paymentsQuery.Where(x => x.Payment.PaymentDate >= options.FromDate.Value);

            if (options.ToDate.HasValue)
                paymentsQuery = paymentsQuery.Where(x => x.Payment.PaymentDate <= options.ToDate.Value);

            if (!options.FromDate.HasValue && !options.ToDate.HasValue && options.LastDays.HasValue)
            {
                var threshold = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-options.LastDays.Value));
                paymentsQuery = paymentsQuery.Where(x => x.Payment.PaymentDate >= threshold);
            }

            if (options.PaymentMethod.HasValue)
                paymentsQuery = paymentsQuery.Where(x => x.Payment.PaymentMethod == options.PaymentMethod.Value);

            if (options.SwiftOnly == true)
                paymentsQuery = paymentsQuery.Where(x => x.Payment.SwiftTransferId != null);

            if (options.MinAmount.HasValue)
                paymentsQuery = paymentsQuery.Where(x => x.Payment.PaidAmount >= options.MinAmount.Value);

            if (options.MaxAmount.HasValue)
                paymentsQuery = paymentsQuery.Where(x => x.Payment.PaidAmount <= options.MaxAmount.Value);

            paymentsQuery = paymentsQuery.OrderByDescending(x => x.Payment.PaymentDate);

            var rows = await paymentsQuery
                .Select(x => new
                {
                    PaymentId = x.Payment.Id,
                    PaymentDate = x.Payment.PaymentDate,
                    PaidAmount = x.Payment.PaidAmount,
                    PaymentMethod = x.Payment.PaymentMethod,
                    SwiftTransferId = x.Payment.SwiftTransferId,
                    DocumentId = x.Document.Id,
                    DocNumber = x.Document.DocNumber,
                    DocDate = x.Document.DocDate,
                    DocDue = x.Document.DocDue,
                    DocTypeId = x.Document.DocTypeId,
                    DocTypeName = x.Document.DocType != null ? x.Document.DocType.Name : null,
                    Side = x.Document.Side,
                    CurrencyCode = x.Document.Currency.CurrencyCode,
                    ExchangeRate = x.Document.Currency.ExchangeRate,
                    SupplierId = x.Document.SupplierId,
                    SupplierName = x.Document.Supplier != null ? x.Document.Supplier.CompanyName : null,
                    BuyerId = x.Document.BuyerId,
                    BuyerName = x.Document.Buyer != null ? x.Document.Buyer.CompanyName : null,
                    VesselId = x.Document.VesselId,
                    VesselName = x.Document.Vessel != null ? x.Document.Vessel.VesselName : null,
                    VoyageId = x.Document.VoyageId,
                    VoyageNumber = x.Document.Voyage != null ? x.Document.Voyage.VoyageNumber : null,
                })
                .ToListAsync(ct);

            var payments = rows
                .Select(r => new PaymentReportRow
                {
                    PaymentId = r.PaymentId,
                    PaymentDate = r.PaymentDate,
                    PaidAmount = r.PaidAmount,
                    PaidAmountBase = r.PaidAmount * r.ExchangeRate / baseRate.ExchangeRate,
                    PaymentMethod = r.PaymentMethod,
                    SwiftTransferId = r.SwiftTransferId,
                    DocumentId = r.DocumentId,
                    DocNumber = r.DocNumber,
                    DocDate = r.DocDate,
                    DocDue = r.DocDue,
                    DocTypeId = r.DocTypeId,
                    DocTypeName = r.DocTypeName,
                    Side = r.Side,
                    CurrencyCode = r.CurrencyCode,
                    SupplierId = r.SupplierId,
                    SupplierName = r.SupplierName,
                    BuyerId = r.BuyerId,
                    BuyerName = r.BuyerName,
                    VesselId = r.VesselId,
                    VesselName = r.VesselName,
                    VoyageId = r.VoyageId,
                    VoyageNumber = r.VoyageNumber,
                })
                .ToList();

            // ─── الموقع المالي الفعلي (cash basis) ───────────────────────────────
            var cashIn = payments.Where(p => p.Side == FinancialSide.Revenue).Sum(p => p.PaidAmountBase);
            var cashOut = payments.Where(p => p.Side == FinancialSide.Expense).Sum(p => p.PaidAmountBase);
            var netCashFlow = cashIn - cashOut;

            var monthlySummary = payments
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new MonthlyPaymentSummary
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    CashIn = g.Where(p => p.Side == FinancialSide.Revenue).Sum(p => p.PaidAmountBase),
                    CashOut = g.Where(p => p.Side == FinancialSide.Expense).Sum(p => p.PaidAmountBase),
                    NetCashFlow = g.Where(p => p.Side == FinancialSide.Revenue).Sum(p => p.PaidAmountBase)
                                - g.Where(p => p.Side == FinancialSide.Expense).Sum(p => p.PaidAmountBase),
                    Count = g.Count(),
                })
                .OrderBy(m => m.Year).ThenBy(m => m.Month)
                .ToList();

            var methodSummary = payments
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new PaymentMethodSummary
                {
                    Method = g.Key.ToString(),
                    TotalBase = g.Sum(p => p.PaidAmountBase),
                    CashIn = g.Where(p => p.Side == FinancialSide.Revenue).Sum(p => p.PaidAmountBase),
                    CashOut = g.Where(p => p.Side == FinancialSide.Expense).Sum(p => p.PaidAmountBase),
                    Count = g.Count(),
                })
                .OrderByDescending(m => m.TotalBase)
                .ToList();

            var vesselSummary = payments
                .Where(p => p.VesselId.HasValue)
                .GroupBy(p => p.VesselId!.Value)
                .Select(g => new VesselPaymentSummary
                {
                    VesselId = g.Key,
                    VesselName = g.First().VesselName ?? string.Empty,
                    CashIn = g.Where(p => p.Side == FinancialSide.Revenue).Sum(p => p.PaidAmountBase),
                    CashOut = g.Where(p => p.Side == FinancialSide.Expense).Sum(p => p.PaidAmountBase),
                    NetCashFlow = g.Where(p => p.Side == FinancialSide.Revenue).Sum(p => p.PaidAmountBase)
                                - g.Where(p => p.Side == FinancialSide.Expense).Sum(p => p.PaidAmountBase),
                    Count = g.Count(),
                })
                .OrderByDescending(v => v.CashIn + v.CashOut)
                .ToList();

            var supplierSummary = payments
                .Where(p => p.SupplierId.HasValue)
                .GroupBy(p => p.SupplierId!.Value)
                .Select(g => new SupplierPaymentSummary
                {
                    SupplierId = g.Key,
                    SupplierName = g.First().SupplierName ?? string.Empty,
                    TotalPaidBase = g.Sum(p => p.PaidAmountBase),
                    Count = g.Count(),
                })
                .OrderByDescending(s => s.TotalPaidBase)
                .ToList();

            var buyerSummary = payments
                .Where(p => p.BuyerId.HasValue)
                .GroupBy(p => p.BuyerId!.Value)
                .Select(g => new BuyerPaymentSummary
                {
                    BuyerId = g.Key,
                    BuyerName = g.First().BuyerName ?? string.Empty,
                    TotalReceivedBase = g.Sum(p => p.PaidAmountBase),
                    Count = g.Count(),
                })
                .OrderByDescending(b => b.TotalReceivedBase)
                .ToList();

            var voyageSummary = payments
                .Where(p => p.VoyageId.HasValue)
                .GroupBy(p => p.VoyageId!.Value)
                .Select(g => new VoyagePaymentSummary
                {
                    VoyageId = g.Key,
                    VoyageNumber = g.First().VoyageNumber ?? string.Empty,
                    CashIn = g.Where(p => p.Side == FinancialSide.Revenue).Sum(p => p.PaidAmountBase),
                    CashOut = g.Where(p => p.Side == FinancialSide.Expense).Sum(p => p.PaidAmountBase),
                    Count = g.Count(),
                })
                .OrderByDescending(v => v.CashIn + v.CashOut)
                .ToList();

            return new PaymentsReport
            {
                From = options.FromDate,
                To = options.ToDate,
                Payments = payments,
                CashIn = cashIn,
                CashOut = cashOut,
                NetCashFlow = netCashFlow,
                MonthlySummary = monthlySummary,
                MethodSummary = methodSummary,
                VesselSummary = vesselSummary,
                SupplierSummary = supplierSummary,
                BuyerSummary = buyerSummary,
                VoyageSummary = voyageSummary,
                Count = payments.Count,
                BaseCurrencyCode = await GetBaseCurrencyCode(ct),
            };
        }


        //===Privates=====================================================


        private async Task<CurrencyResponse> GetBaseCurrencyExchangeRate(CancellationToken ct = default)
        {
            var result = await _currencyRepo.Query()
                .AsNoTracking()
                .Where(c => c.IsBaseCurrency == true)
                .Select(r => new CurrencyResponse
                {
                    Code = r.CurrencyCode,
                    ExchangeRate = r.ExchangeRate,
                })
                .FirstOrDefaultAsync(ct);

            if (result == null)
                throw new InvalidOperationException("there is not Base Currency was set");

            return result;
        }

        private async Task<string> GetBaseCurrencyCode(CancellationToken ct = default)
        {
            return await _currencyRepo.Query()
                .AsNoTracking()
                .Where(b => b.IsBaseCurrency == true)
                .Select(bc => bc.CurrencyCode)
                .FirstOrDefaultAsync(ct) ?? "";
        }

        private async Task<Document> GetWithPaymentsOrThrowAsync(int id, CancellationToken ct)
            => await _repo.Query()
                   .Include(x => x.Payments)
                   .FirstOrDefaultAsync(x => x.Id == id, ct)
               ?? throw new KeyNotFoundException($"Document {id} not found.");
    }
}
