using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Options;
using Microsoft.Extensions.Options;

namespace Marilog.Application.Helpers
{
    public sealed class ExchangeRateRefreshGate
    {
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly TimeSpan _minInterval;
        private DateTime? _lastSuccessUtc;

        public ExchangeRateRefreshGate(IOptions<ExchangeRatesOptions> options)
            => _minInterval = TimeSpan.FromMinutes(Math.Max(options.Value.MinRefreshIntervalMinutes, 1));

        public async Task<RefreshRatesResponse> RunAsync(
            Func<Task<RefreshRatesResponse>> refresh, CancellationToken ct)
        {
            await _lock.WaitAsync(ct);
            try
            {
                if (_lastSuccessUtc is { } last && DateTime.UtcNow - last < _minInterval)
                {
                    return new RefreshRatesResponse
                    {
                        IsThrottled = true,
                        NextAllowedAtUtc = last + _minInterval
                    };
                }

                var result = await refresh();
                _lastSuccessUtc = DateTime.UtcNow;
                result.NextAllowedAtUtc = _lastSuccessUtc.Value + _minInterval;
                return result;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}