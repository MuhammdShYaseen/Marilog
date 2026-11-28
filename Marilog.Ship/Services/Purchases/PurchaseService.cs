using Marilog.Contracts.DTOs.Requests.DocumentDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Marilog.Ship.Domain;
using Marilog.Ship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Ship.Services.Purchases
{
    /// <summary>
    /// Ship purchases are Marilog Documents: this only fills the ship's defaults and
    /// calls DocumentService / PaymentService, which keep every rule.
    /// </summary>
    public class PurchaseService : IPurchaseService
    {
        private const string NoNumberPrefix = "NR-";

        private readonly IDocumentService _documentService;
        private readonly IPaymentService _paymentService;
        private readonly IRepository<Document> _documentRepo;
        private readonly IRepository<DocumentType> _docTypeRepo;
        private readonly IRepository<ShipSetup> _setupRepo;

        public PurchaseService(
            IDocumentService documentService,
            IPaymentService paymentService,
            IRepository<Document> documentRepo,
            IRepository<DocumentType> docTypeRepo,
            IRepository<ShipSetup> setupRepo)
        {
            _documentService = documentService;
            _paymentService = paymentService;
            _documentRepo = documentRepo;
            _docTypeRepo = docTypeRepo;
            _setupRepo = setupRepo;
        }

        public async Task<IReadOnlyList<DocumentResponse>> GetAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
        {
            var (vesselId, _) = await GetVesselAsync(ct);
            var documents = await _documentService.GetByVesselAsync(vesselId, ct: ct);

            return documents
                .Where(x => x.DocDate >= from && x.DocDate <= to)
                .OrderByDescending(x => x.DocDate)
                .ThenByDescending(x => x.Id)
                .ToList();
        }

        public async Task<int> CreateAsync(CreatePurchaseRequest request, CancellationToken ct = default)
        {
            if (request.Amount <= 0)
                throw new InvalidOperationException("The amount must be more than zero.");

            if (request.PaidWith == PaymentMethod.CigarettesOnBoard && await IsCigarettesTypeAsync(request.DocTypeId, ct))
                throw new InvalidOperationException("Cigarettes can't be paid for with cigarettes on board.");

            var (vesselId, ownerCompanyId) = await GetVesselAsync(ct);

            var docNumber = string.IsNullOrWhiteSpace(request.DocNumber)
                ? await NextNoNumberAsync(ct)
                : request.DocNumber.Trim();

            await _documentService.CreateAsync(new CreateDocumentRequest
            {
                DocNumber = docNumber,
                DocTypeId = request.DocTypeId,
                DocDate = request.DocDate,
                CurrencyId = request.CurrencyId,
                TotalAmount = request.Amount,
                SupplierId = request.SupplierId,
                BuyerId = ownerCompanyId,
                VesselId = vesselId,
                PortId = request.PortId,
                Reference = string.IsNullOrWhiteSpace(request.Reference) ? null : request.Reference.Trim(),
                Side = FinancialSide.Expense
            }, ct);

            // DocumentService.CreateAsync does not return the new Id.
            var documentId = await _documentRepo.Query()
                .Where(x => x.DocNumber == docNumber)
                .Select(x => x.Id)
                .FirstAsync(ct);

            if (request.PaidWith is { } method)
            {
                await _paymentService.AddPaymentAsync(documentId, new AddPaymentRequest
                {
                    PaidAmount = request.Amount,
                    PaymentDate = request.DocDate,
                    Method = method
                }, ct);
            }

            return documentId;
        }

        // ── Private ──────────────────────────────────────────────────────────────

        private async Task<(int VesselId, int OwnerCompanyId)> GetVesselAsync(CancellationToken ct)
        {
            var vessel = await _setupRepo.Query().AsNoTracking()
                .Select(x => new { x.VesselId, x.Vessel.CompanyID })
                .FirstOrDefaultAsync(ct)
                ?? throw new InvalidOperationException("Complete the initial setup first.");

            return (vessel.VesselId, vessel.CompanyID);
        }

        private Task<bool> IsCigarettesTypeAsync(int docTypeId, CancellationToken ct) =>
            _docTypeRepo.Query().AnyAsync(x => x.Id == docTypeId && x.Code == ShipCodes.CigarettesDocType, ct);

        // Receipts without a number: NR-0001, NR-0002, ...
        // Deleted documents are counted too — their number is still taken in the unique index.
        private async Task<string> NextNoNumberAsync(CancellationToken ct)
        {
            var numbers = await _documentRepo.Query()
                .IgnoreQueryFilters()
                .Where(x => x.DocNumber.StartsWith(NoNumberPrefix))
                .Select(x => x.DocNumber)
                .ToListAsync(ct);

            var last = numbers
                .Select(n => int.TryParse(n[NoNumberPrefix.Length..], out var value) ? value : 0)
                .DefaultIfEmpty(0)
                .Max();

            return $"{NoNumberPrefix}{last + 1:D4}";
        }
    }
}