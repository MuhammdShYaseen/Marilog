using Marilog.Domain.Common;

namespace Marilog.Domain.Entities
{
    public enum DocumentType
    {
        Invoice = 0,
        DeliveryNote = 1,
        Quotation = 2,

    }
    public class Document : Entity
    {
        public string DocNumber { get; private set; } = null!;
        public DocumentType DocType { get; private set; }
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

        private readonly List<DocumentItem> _items = new();
        private readonly List<Payment> _payments = new();

        public IReadOnlyCollection<DocumentItem> Items => _items.AsReadOnly();
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();


        public static Document Create(string docNumber, DocumentType docType, DateOnly docDate,
            int currencyId, decimal totalAmount, int? supplierId = null, int? buyerId = null,
            int? vesselId = null, int? portId = null,
            string? reference = null, string? filePath = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(docNumber);
            
            if (totalAmount < 0) throw new ArgumentException("TotalAmount cannot be negative.");

            return new Document
            {
                DocNumber = docNumber,
                DocType = docType,
                DocDate = docDate,
                CurrencyId = currencyId,
                TotalAmount = totalAmount,
                SupplierId = supplierId,
                BuyerId = buyerId,
                VesselId = vesselId,
                PortId = portId,
                Reference = reference,
                FilePath = filePath
            };
        }

        public void Update(DocumentType docType, DateOnly docDate, int currencyId,
            decimal totalAmount, int? supplierId = null, int? buyerId = null,
            int? vesselId = null, int? portId = null,
            string? reference = null, string? filePath = null)
        {
            if (totalAmount < 0) throw new ArgumentException("TotalAmount cannot be negative.");

            DocType = docType;
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

        // ── Items ───────────────────────────────────────────────────────────────
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

        // ── Payments ────────────────────────────────────────────────────────────
        public Payment AddPayment(int swiftTransferId, decimal paidAmount, DateOnly paymentDate)
        {
            if (RemainingBalance() < paidAmount)
                throw new InvalidOperationException("PaidAmount exceeds remaining balance.");

            var payment = Payment.Create(Id, swiftTransferId, paidAmount, paymentDate);
            _payments.Add(payment);
            Touch();
            return payment;
        }

        // ── Computed ────────────────────────────────────────────────────────────
        public decimal TotalPaid() => _payments.Sum(p => p.PaidAmount);
        public decimal RemainingBalance() => TotalAmount - TotalPaid();
        public bool IsFullyPaid() => RemainingBalance() <= 0;

        private void RecalculateTotal() =>
            TotalAmount = _items.Sum(i => i.LineTotal);
    }
}
