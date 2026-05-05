using System;
using System.Security.Cryptography;
using System.Text;

namespace StreamingPlatform.Auth
{
    /// <summary>
    /// Hash de parolă cu HMACSHA512 + salt aleator pe 128 bytes.
    /// Schema VARBINARY(256) pentru hash și VARBINARY(128) pentru salt
    /// din StreamZoneDB se mapează exact pe acest algoritm.
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltLengthBytes = 128;

        public static (byte[] hash, byte[] salt) Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltLengthBytes);
            using var hmac = new HMACSHA512(salt);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return (hash, salt);
        }

        public static bool Verify(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new HMACSHA512(storedSalt);
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return CryptographicOperations.FixedTimeEquals(computed, storedHash);
        }
    }
}
