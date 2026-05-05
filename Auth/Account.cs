using System;
using System.Collections.Generic;

namespace StreamingPlatform.Auth
{
    /// <summary>
    /// Mapează tabelul Users din StreamZoneDB.
    /// Reprezintă contul (email + parolă), NU profilul vizionărilor.
    /// </summary>
    public class Account
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public bool EmailConfirmed { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEndUtc { get; set; }

        public List<UserProfile> Profiles { get; set; } = new();
        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
