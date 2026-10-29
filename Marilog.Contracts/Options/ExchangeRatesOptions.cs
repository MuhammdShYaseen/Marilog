namespace Marilog.Contracts.Options
{
    public sealed class ExchangeRatesOptions
    {
        public const string SectionName = "ExchangeRates";
        public string BaseUrl { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
        public int MinRefreshIntervalMinutes { get; set; } = 60;
    }
}
