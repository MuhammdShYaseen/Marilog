using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public class CurrencyTickerItemResponse
    {
        public string Code { get; set; } = string.Empty;
        public decimal RateInBase { get; set; }           // 1 unit = RateInBase of base currency
        public decimal? PreviousRateInBase { get; set; }  // yesterday's close
        public decimal? ChangePercent { get; set; }
        public string BaseCurrencyCode { get; set; } = string.Empty;
        public RateTrend Trend { get; set; }
    }
}
