namespace Marilog.Domain.ValueObjects.Cargo
{
    /// <summary>General cargo extension of a cargo line. All fields optional.</summary>
    public sealed class GeneralCargoDetail
    {
        public int? PackageCount { get; private set; }
        public string? PackageType { get; private set; }   // bags, pallets, coils, crates...
        public decimal? VolumeCbm { get; private set; }

        private GeneralCargoDetail() { }   // EF Core

        /// <summary>Returns null when all fields are empty — no empty extension rows.</summary>
        public static GeneralCargoDetail? Create(int? packageCount, string? packageType, decimal? volumeCbm)
        {
            if (packageCount < 0)
                throw new ArgumentException("Package count cannot be negative.");

            if (volumeCbm < 0)
                throw new ArgumentException("Volume cannot be negative.");

            var type = string.IsNullOrWhiteSpace(packageType) ? null : packageType.Trim();

            if (packageCount is null && type is null && volumeCbm is null)
                return null;

            return new GeneralCargoDetail
            {
                PackageCount = packageCount,
                PackageType = type,
                VolumeCbm = volumeCbm
            };
        }
    }
}