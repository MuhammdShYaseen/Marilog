using Marilog.Domain.Common;
using Marilog.Domain.Events;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class Document : Entity
    {
        public string DocNumber { get; private set; } = null!;
        public int DocTypeId { get; private set; }
        public DocumentType DocType { get; private set; } = null!;
        public DateOnly DocDate { get; private set; }

        public int? SupplierId { get; private set; }
        public Company? Supplier { get; private set; }
        public int? BuyerId { get; private set; }
        public Company? Buyer { get; private set; }
        public int? VesselId { get; private set; }
        public Vessel? Vessel { get; private set; }
        public int? PortId { get; private set; }
        public Port? Port { get; private set; }

        public int CurrencyId { get; private set; }
        public Currency Currency { get; private set; } = null!;

        public decimal TotalAmount { get; private set; }
        public string? Reference { get; private set; }
        public string? FilePath { get; private set; }

        // ── Parent reference only — no navigation ownership ──────────────────────
        public int? ParentDocumentId { get; private set; }

        private readonly List<DocumentItem> _items = new();
        private readonly List<Payment> _payments = new();

        public IReadOnlyCollection<DocumentItem> Items => _items.AsReadOnly();
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

        private Document() { }

        // ── Factory ──────────────────────────────────────────────────────────────
        public static Document Create(
            string docNumber,
            int docTypeId,
            DateOnly docDate,
            int currencyId,
            decimal totalAmount,
            int? supplierId = null,
            int? buyerId = null,
            int? vesselId = null,
            int? portId = null,
            int? parentDocumentId = null,
            string? reference = null,
            string? filePath = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(docNumber);
            if (docTypeId <= 0) throw new ArgumentException("Invalid DocTypeId.");
            if (currencyId <= 0) throw new ArgumentException("Invalid CurrencyId.");
            if (totalAmount < 0) throw new ArgumentException("TotalAmount cannot be negative.");

            return new Document
            {
                DocNumber = docNumber,
                DocTypeId = docTypeId,
                DocDate = docDate,
                CurrencyId = currencyId,
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

        // ── Update ───────────────────────────────────────────────────────────────
        public void Update(
            int docTypeId,
            DateOnly docDate,
            int currencyId,
            decimal totalAmount,
            int? supplierId = null,
            int? buyerId = null,
            int? vesselId = null,
            int? portId = null,
            string? reference = null,
            string? filePath = null)
        {
            if (docTypeId <= 0) throw new ArgumentException("Invalid DocTypeId.");
            if (currencyId <= 0) throw new ArgumentException("Invalid CurrencyId.");
            if (totalAmount < 0) throw new ArgumentException("TotalAmount cannot be negative.");

            DocTypeId = docTypeId;
            DocDate = docDate;
            CurrencyId = currencyId;
            TotalAmount = totalAmount;
            SupplierId = supplierId;
            BuyerId = buyerId;
            VesselId = vesselId;
            PortId = portId;
            Reference = reference;
            FilePath = filePath;
            Touch();
        }

        // ── Parent linking ────────────────────────────────────────────────────────
        public void LinkToParent(int parentDocumentId)
        {
            if (parentDocumentId == Id)
                throw new InvalidOperationException("Document cannot be its own parent.");
            ParentDocumentId = parentDocumentId;
            Touch();
        }

        public void UnlinkFromParent()
        {
            ParentDocumentId = null;
            Touch();
        }

        // ── Items ────────────────────────────────────────────────────────────────
        public DocumentItem AddItem(string productName, decimal quantity,
            decimal unitPrice, string? unit = null)
        {
            var item = DocumentItem.Create(Id, productName, quantity, unitPrice, unit);
            _items.Add(item);
            RecalculateTotal();
            Touch();
            return item;
        }

        public void UpdateItem(int itemId, string productName, decimal quantity,
            decimal unitPrice, string? unit = null)
        {
            var item = _items.FirstOrDefault(x => x.Id == itemId)
                ?? throw new InvalidOperationException($"Item {itemId} not found.");
            item.Update(productName, quantity, unitPrice, unit);
            RecalculateTotal();
            Touch();
        }

        public void RemoveItem(int itemId)
        {
            var item = _items.FirstOrDefault(x => x.Id == itemId)
                ?? throw new InvalidOperationException($"Item {itemId} not found.");
            _items.Remove(item);
            RecalculateTotal();
            Touch();
        }

        // ── Payments ─────────────────────────────────────────────────────────────
        public Payment AddPayment(int swiftTransferId, decimal paidAmount, DateOnly paymentDate)
        {
            if (paidAmount <= 0)
                throw new ArgumentException("PaidAmount must be positive.");
            if (RemainingBalance < paidAmount)
                throw new InvalidOperationException("PaidAmount exceeds remaining balance.");

            var payment = Payment.Create(Id, swiftTransferId, paidAmount, paymentDate);
            _payments.Add(payment);
            Touch();

            if (IsFullyPaid)
                AddDomainEvent(new DocumentFullyPaidEvent(Id));

            return payment;
        }

        // ── Email — raises Domain Event; handler persists Email aggregate ─────────
        /// <summary>
        /// Logs an email exchange as part of the procurement audit trail.
        /// Participants must include exactly one From and at least one To.
        /// Does NOT own the Email entity — persistence is handled by the event handler.
        /// </summary>
        public void LogEmail(
            string subject,
            string body,
            IReadOnlyList<EmailParticipantData> participants,
            EmailDirection direction = EmailDirection.Outbound)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(subject);
            ArgumentException.ThrowIfNullOrWhiteSpace(body);

            if (!participants.Any(p => p.Role == ParticipantRole.From))
                throw new InvalidOperationException("Email must have a sender.");
            if (!participants.Any(p => p.Role == ParticipantRole.To))
                throw new InvalidOperationException("Email must have at least one recipient.");

            AddDomainEvent(new DocumentEmailRequestedEvent(
                DocumentId: Id,
                DocNumber: DocNumber,
                EntityType: nameof(Document),
                Subject: subject,
                Body: body,
                Direction: direction,
                Participants: participants));
        }

        // ── Computed ─────────────────────────────────────────────────────────────
        public decimal TotalPaid => _payments.Sum(p => p.PaidAmount);
        public decimal RemainingBalance => TotalAmount - TotalPaid;
        public bool IsFullyPaid => RemainingBalance <= 0;

        private void RecalculateTotal() => TotalAmount = _items.Sum(i => i.LineTotal);
    }
}