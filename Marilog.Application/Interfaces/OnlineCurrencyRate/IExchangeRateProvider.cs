namespace Marilog.Application.Interfaces.OnlineCurrencyRate
{
    public interface IExchangeRateProvider
    {
        Task<IReadOnlyDictionary<string, decimal>> GetLatestAsync(IEnumerable<string> codes, CancellationToken ct = default);
        Task<IReadOnlyDictionary<string, decimal>> GetHistoricalAsync(DateOnly date, IEnumerable<string> codes, CancellationToken ct = default);
    }
}
