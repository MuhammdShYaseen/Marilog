namespace Marilog.Contracts.Options
{
    /// <summary>Company secret for the ship program's unlock codes (appsettings: "ShipUnlock").</summary>
    public sealed class ShipUnlockOptions
    {
        public const string SectionName = "ShipUnlock";
        public string Secret { get; set; } = string.Empty;
    }
}