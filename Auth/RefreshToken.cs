using System;

namespace StreamingPlatform.Auth
{
    /// <summary>
    /// Mapează tabelul RefreshTokens din StreamZoneDB.
    /// Token-uri de durată lungă pe care frontend-ul le folosește pentru a
    /// reînnoi sesiunile fără re-login.
    /// </summary>
    public class RefreshToken
    {
        public int TokenId { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }

        public Account? Account { get; set; }

        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
    }
}
