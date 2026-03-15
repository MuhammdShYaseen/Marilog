namespace Marilog.Domain.Entities
{
    public class DocumentItem
    {
        public int Id { get; private set; }
        public int DocumentId { get; private set; }
        public string ProductName { get; private set; } = null!;
        public decimal Quantity { get; private set; }
        public string? Unit { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal LineTotal { get; private set; }

        private DocumentItem() { }
        internal static DocumentItem Create(int documentId, string productName,
            decimal quantity, decimal unitPrice, string? unit = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(productName);
            if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
            if (unitPrice < 0) throw new ArgumentException("UnitPrice cannot be negative.");

            return new DocumentItem
            {
                DocumentId = documentId,
                ProductName = productName,
                Quantity = quantity,
                Unit = unit,
                UnitPrice = unitPrice,
                LineTotal = Math.Round(quantity * unitPrice, 2)
            };
        }

        internal void Update(string productName, decimal quantity,
            decimal unitPrice, string? unit = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(productName);
            if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
            if (unitPrice < 0) throw new ArgumentException("UnitPrice cannot be negative.");

            ProductName = productName;
            Quantity = quantity;
            Unit = unit;
            UnitPrice = unitPrice;
            LineTotal = Math.Round(quantity * unitPrice, 2);
        }
    }
}