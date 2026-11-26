namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    /// <summary>Ship keys and one-time unlock codes for the ship program (Marilog.Ship).</summary>
    public interface IShipUnlockService
    {
        /// <summary>The key typed into the ship program once, at installation.</summary>
        Task<string> GetShipKeyAsync(string imoNumber, CancellationToken ct = default);

        /// <summary>The 6-digit code that unlocks one edit for the request code shown on board.</summary>
        Task<string> GetUnlockCodeAsync(string imoNumber, string requestCode, CancellationToken ct = default);
    }
}