namespace Marilog.Contracts.DTOs.Requests.CurrencyDTOs
{
    public class CreateCurrencyRequest
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public decimal ExchangeRate { get; set; }
        public string? Symbol { get; set; }
    }
}
