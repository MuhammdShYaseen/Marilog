namespace Marilog.Contracts.DTOs.Reports.DocumentReports
{
    // Open balances as of today, split by how late they are. Base currency. No filters.
    // A document with no due date counts as due on its document date.
    public class AgingReport
    {
        public DateOnly AsOf { get; set; }
        public string BaseCurrencyCode { get; set; } = string.Empty;

        public IReadOnlyList<AgingRow> Expenses { get; set; } = [];   // expense docs, by supplier
        public IReadOnlyList<AgingRow> Revenues { get; set; } = [];   // revenue docs, by buyer
    }

    // One company's open balance.
    public class AgingRow
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;

        public decimal NotDue { get; set; }      // due today or later
        public decimal Days1To30 { get; set; }
        public decimal Days31To60 { get; set; }
        public decimal Days61To90 { get; set; }
        public decimal Over90 { get; set; }

        public decimal Total => NotDue + Days1To30 + Days31To60 + Days61To90 + Over90;
    }
}