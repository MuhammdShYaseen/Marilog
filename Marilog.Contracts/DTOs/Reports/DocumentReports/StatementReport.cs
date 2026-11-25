namespace Marilog.Contracts.DTOs.Reports.DocumentReports
{
    // Statement of account between a seller and a buyer, in base currency.
    // Balance = what the buyer owes the seller (negative = the seller owes the buyer).
    public class StatementReport
    {
        public string SellerName { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
        public string BaseCurrencyCode { get; set; } = string.Empty;

        public decimal OpeningBalance { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal ClosingBalance { get; set; }

        public IReadOnlyList<StatementLine> Lines { get; set; } = [];
    }

    public class StatementLine
    {
        public DateOnly Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public int DocumentId { get; set; }
        public decimal Debit { get; set; }     // increases what the buyer owes
        public decimal Credit { get; set; }    // decreases what the buyer owes
        public decimal Balance { get; set; }   // running balance after this line
    }
}