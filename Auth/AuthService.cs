using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace StreamingPlatform.Auth
{
    public record AuthResult(
        bool Success,
        string? Message,
        int? UserId,
        string? Username,
        string? Email,
        string? AccessToken,
        string? RefreshToken,
        DateTime? AccessTokenExpiresAt
    );

    public class AuthService
    {
        private readonly StreamZoneDbContext _db;
        private readonly int _accessLifetimeMinutes;
        private readonly int _refreshLifetimeDays;
        private readonly int _maxFailedAttempts;
        private readonly int _lockoutMinutes;

        public AuthService(StreamZoneDbContext db, IConfiguration config)
        {
            _db = db;
            _accessLifetimeMinutes = config.GetValue("Auth:AccessTokenLifetimeMinutes", 60);
            _refreshLifetimeDays = config.GetValue("Auth:RefreshTokenLifetimeDays", 30);
            _maxFailedAttempts = config.GetValue("Auth:MaxFailedLoginAttempts", 5);
            _lockoutMinutes = config.GetValue("Auth:LockoutMinutes", 15);
        }

        // ── REGISTER ─────────────────────────────────────────────────────────
        public async Task<AuthResult> RegisterAsync(string email, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return new AuthResult(false, "Email, username și parolă sunt obligatorii.", null, null, null, null, null, null);

            if (password.Length < 6)
                return new AuthResult(false, "Parola trebuie să aibă cel puțin 6 caractere.", null, null, null, null, null, null);

            email = email.Trim().ToLowerInvariant();

            if (await _db.Accounts.AnyAsync(a => a.Email == email))
                return new AuthResult(false, "Există deja un cont cu acest email.", null, null, null, null, null, null);

            if (await _db.Accounts.AnyAsync(a => a.Username == username))
                return new AuthResult(false, "Username-ul este deja folosit.", null, null, null, null, null, null);

            var (hash, salt) = PasswordHasher.Hash(password);
            var account = new Account
            {
                Email = email,
                Username = username,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = false
            };

            _db.Accounts.Add(account);
            await _db.SaveChangesAsync();

            return new AuthResult(true, "Cont creat cu succes.", account.UserId, account.Username, account.Email, null, null, null);
        }

        // ── LOGIN ────────────────────────────────────────────────────────────
        public async Task<AuthResult> LoginAsync(string emailOrUsername, string password)
        {
            if (string.IsNullOrWhiteSpace(emailOrUsername) || string.IsNullOrWhiteSpace(password))
                return new AuthResult(false, "Date de login lipsă.", null, null, null, null, null, null);

            var key = emailOrUsername.Trim();
            var keyLower = key.ToLowerInvariant();

            var account = await _db.Accounts
                .FirstOrDefaultAsync(a => a.Email == keyLower || a.Username == key);

            if (account == null)
                return new AuthResult(false, "Email/username sau parolă incorectă.", null, null, null, null, null, null);

            if (!account.IsActive)
                return new AuthResult(false, "Contul este dezactivat.", null, null, null, null, null, null);

            if (account.LockoutEndUtc.HasValue && account.LockoutEndUtc.Value > DateTime.UtcNow)
            {
                var minutes = (int)Math.Ceiling((account.LockoutEndUtc.Value - DateTime.UtcNow).TotalMinutes);
                return new AuthResult(false, $"Cont blocat. Reîncearcă în ~{minutes} min.", null, null, null, null, null, null);
            }

            if (!PasswordHasher.Verify(password, account.PasswordHash, account.PasswordSalt))
            {
                account.FailedLoginAttempts++;
                if (account.FailedLoginAttempts >= _maxFailedAttempts)
                {
                    account.LockoutEndUtc = DateTime.UtcNow.AddMinutes(_lockoutMinutes);
                    account.FailedLoginAttempts = 0;
                    await _db.SaveChangesAsync();
                    return new AuthResult(false, $"Prea multe încercări greșite. Cont blocat {_lockoutMinutes} min.", null, null, null, null, null, null);
                }
                await _db.SaveChangesAsync();
                return new AuthResult(false, "Email/username sau parolă incorectă.", null, null, null, null, null, null);
            }

            // Login OK
            account.FailedLoginAttempts = 0;
            account.LockoutEndUtc = null;
            account.LastLoginAt = DateTime.UtcNow;

            var accessExpires = DateTime.UtcNow.AddMinutes(_accessLifetimeMinutes);
            var refresh = new RefreshToken
            {
                UserId = account.UserId,
                Token = GenerateOpaqueToken(64),
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshLifetimeDays),
                CreatedAt = DateTime.UtcNow
            };
            _db.RefreshTokens.Add(refresh);
            await _db.SaveChangesAsync();

            return new AuthResult(true, "Autentificare reușită.", account.UserId, account.Username, account.Email,
                GenerateOpaqueToken(48), refresh.Token, accessExpires);
        }

        // ── REFRESH ──────────────────────────────────────────────────────────
        public async Task<AuthResult> RefreshAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return new AuthResult(false, "Token lipsă.", null, null, null, null, null, null);

            var existing = await _db.RefreshTokens
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (existing == null || !existing.IsActive || existing.Account == null || !existing.Account.IsActive)
                return new AuthResult(false, "Token invalid sau expirat.", null, null, null, null, null, null);

            // Rotate refresh token (revocă vechiul, creează unul nou)
            existing.RevokedAt = DateTime.UtcNow;
            var newRefresh = new RefreshToken
            {
                UserId = existing.UserId,
                Token = GenerateOpaqueToken(64),
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshLifetimeDays),
                CreatedAt = DateTime.UtcNow
            };
            _db.RefreshTokens.Add(newRefresh);
            await _db.SaveChangesAsync();

            var accessExpires = DateTime.UtcNow.AddMinutes(_accessLifetimeMinutes);
            return new AuthResult(true, "Token reînnoit.", existing.Account.UserId, existing.Account.Username,
                existing.Account.Email, GenerateOpaqueToken(48), newRefresh.Token, accessExpires);
        }

        // ── LOGOUT ───────────────────────────────────────────────────────────
        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var existing = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
            if (existing == null || existing.RevokedAt != null) return false;
            existing.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        private static string GenerateOpaqueToken(int bytes)
        {
            var buf = RandomNumberGenerator.GetBytes(bytes);
            return Convert.ToBase64String(buf).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
    }
}
