using System.Security.Cryptography;
using System.Text;

namespace Marilog.Kernel.Rules
{
    /// <summary>
    /// Offline unlock codes for the ship program — shared by Marilog (office) and Marilog.Ship.
    ///
    ///   • Ship key   = HMAC(company secret, IMO). Shown in Marilog, typed once into the ship program.
    ///   • Request    = 6 random digits created on board for one edit.
    ///   • Unlock code= HMAC(ship key, request) → 6 digits. Valid for that request only.
    /// </summary>
    public static class ShipUnlockRules
    {
        // 32 characters without I, O, 0, 1 (easy to read over the phone).
        private const string KeyAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int KeyLength = 16;
        private const int CodeDigits = 6;

        /// <summary>Ship key for one vessel, formatted XXXX-XXXX-XXXX-XXXX.</summary>
        public static string ShipKey(string companySecret, string imoNumber)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(companySecret);
            ArgumentException.ThrowIfNullOrWhiteSpace(imoNumber);

            var hash = Hmac(companySecret, "SHIP|" + NormalizeImo(imoNumber));
            var chars = new char[KeyLength];
            for (var i = 0; i < KeyLength; i++)
                chars[i] = KeyAlphabet[hash[i] % KeyAlphabet.Length];   // 256 % 32 == 0 → no bias

            var key = new string(chars);
            return $"{key[..4]}-{key[4..8]}-{key[8..12]}-{key[12..]}";
        }

        public static string NewRequestCode() =>
            RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        public static string UnlockCode(string shipKey, string requestCode)
        {
            if (!IsValidKey(shipKey)) throw new ArgumentException("Invalid ship key.");
            if (!IsValidCode(requestCode)) throw new ArgumentException("The request code must be 6 digits.");

            var hash = Hmac(NormalizeKey(shipKey), "UNLOCK|" + requestCode.Trim());
            var value = ((uint)hash[0] << 24 | (uint)hash[1] << 16 | (uint)hash[2] << 8 | hash[3]) % 1_000_000;
            return value.ToString("D6");
        }

        public static bool IsUnlockCodeCorrect(string shipKey, string requestCode, string unlockCode)
        {
            if (!IsValidKey(shipKey) || !IsValidCode(requestCode) || !IsValidCode(unlockCode))
                return false;

            var expected = Encoding.ASCII.GetBytes(UnlockCode(shipKey, requestCode));
            var actual = Encoding.ASCII.GetBytes(unlockCode.Trim());
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }

        public static bool IsValidKey(string? shipKey)
        {
            if (string.IsNullOrWhiteSpace(shipKey)) return false;
            var key = NormalizeKey(shipKey);
            return key.Length == KeyLength && key.All(KeyAlphabet.Contains);
        }

        public static bool IsValidCode(string? code) =>
            code is not null && code.Trim().Length == CodeDigits && code.Trim().All(char.IsAsciiDigit);

        /// <summary>"abcd-efgh ..." → "ABCDEFGH..."</summary>
        public static string NormalizeKey(string shipKey) =>
            shipKey.Replace("-", string.Empty).Replace(" ", string.Empty).Trim().ToUpperInvariant();

        // "IMO 9123456" → "9123456"
        private static string NormalizeImo(string imoNumber)
        {
            var imo = imoNumber.Replace(" ", string.Empty).Trim().ToUpperInvariant();
            return imo.StartsWith("IMO") ? imo[3..] : imo;
        }

        private static byte[] Hmac(string key, string message) =>
            HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(message));
    }
}