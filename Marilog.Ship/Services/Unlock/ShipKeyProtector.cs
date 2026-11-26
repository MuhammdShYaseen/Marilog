using System.Security.Cryptography;
using System.Text;

namespace Marilog.Ship.Services.Unlock
{
    /// <summary>
    /// Keeps the ship key unreadable in ship.db when the file is opened with a database browser.
    /// The AES key is built into the program, so the database still works on another PC.
    /// Not protection against someone who decompiles the program.
    /// </summary>
    internal static class ShipKeyProtector
    {
        private static readonly byte[] AesKey = SHA256.HashData(Encoding.UTF8.GetBytes("Marilog.Ship|ship-key|v1"));

        public static string Protect(string value)
        {
            using var aes = Aes.Create();
            aes.Key = AesKey;
            var iv = RandomNumberGenerator.GetBytes(16);
            var cipher = aes.EncryptCbc(Encoding.UTF8.GetBytes(value), iv);
            return Convert.ToBase64String(iv.Concat(cipher).ToArray());
        }

        public static string Unprotect(string protectedValue)
        {
            var data = Convert.FromBase64String(protectedValue);
            using var aes = Aes.Create();
            aes.Key = AesKey;
            var plain = aes.DecryptCbc(data.AsSpan(16), data.AsSpan(0, 16));
            return Encoding.UTF8.GetString(plain);
        }
    }
}