namespace Marilog.Contracts.DTOs.Requests.CurrencyDTOs
{
    public class UpdateCurrencyRequest
    {
        public string Name { get; set; } = default!;
        public decimal ExchangeRate { get; set; }
        public string? Symbol { get; set; }
    }
}
