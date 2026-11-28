namespace Marilog.Ship.Domain
{
    internal static class ShipGuard
    {
        public static void PositiveMoney(decimal value, string name)
        {
            if (value <= 0)
                throw new ArgumentException($"{name} must be greater than zero.");
            if (decimal.Round(value, 2) != value)
                throw new ArgumentException($"{name} cannot have more than 2 decimal places.");
        }

        public static void Money(decimal value, string name)
        {
            if (decimal.Round(value, 2) != value)
                throw new ArgumentException($"{name} cannot have more than 2 decimal places.");
        }

        public static void Id(int value, string name)
        {
            if (value <= 0)
                throw new ArgumentException($"Invalid {name}.");
        }

        public static string? Text(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}