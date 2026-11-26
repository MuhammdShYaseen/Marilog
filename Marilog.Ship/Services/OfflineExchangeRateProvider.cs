using Marilog.Application.Interfaces.OnlineCurrencyRate;

namespace Marilog.Ship.Services
{
    /// <summary>
    /// The ship has no internet: exchange rates are entered by hand.
    /// Registered only so CurrencyService can be used on board.
    /// </summary>
    public class OfflineExchangeRateProvider : IExchangeRateProvider
    {
        private const string Message = "Online exchange rates are not available on board. Enter the rate by hand.";

        public Task<IReadOnlyDictionary<string, decimal>> GetLatestAsync(
            IEnumerable<string> codes, CancellationToken ct = default)
            => throw new InvalidOperationException(Message);

        public Task<IReadOnlyDictionary<string, decimal>> GetHistoricalAsync(
            DateOnly date, IEnumerable<string> codes, CancellationToken ct = default)
            => throw new InvalidOperationException(Message);
    }
}