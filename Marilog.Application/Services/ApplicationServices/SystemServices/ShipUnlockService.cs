using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Contracts.Options;
using Marilog.Kernel.Rules;
using Microsoft.Extensions.Options;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class ShipUnlockService : IShipUnlockService
    {
        private readonly ShipUnlockOptions _options;

        public ShipUnlockService(IOptions<ShipUnlockOptions> options) => _options = options.Value;

        public Task<string> GetShipKeyAsync(string imoNumber, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(imoNumber);
            return Task.FromResult(ShipUnlockRules.ShipKey(Secret(), imoNumber));
        }

        public Task<string> GetUnlockCodeAsync(string imoNumber, string requestCode, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(imoNumber);
            if (!ShipUnlockRules.IsValidCode(requestCode))
                throw new InvalidOperationException("The request code must be 6 digits.");

            var shipKey = ShipUnlockRules.ShipKey(Secret(), imoNumber);
            return Task.FromResult(ShipUnlockRules.UnlockCode(shipKey, requestCode));
        }

        private string Secret() =>
            string.IsNullOrWhiteSpace(_options.Secret)
                ? throw new InvalidOperationException("ShipUnlock:Secret is not set in appsettings.json.")
                : _options.Secret;
    }
}