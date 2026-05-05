using Microsoft.EntityFrameworkCore;

namespace StreamingPlatform.Auth
{
    /// <summary>
    /// DbContext pentru baza de date StreamZoneDB.
    /// Mapează clasele Account/UserProfile/RefreshToken la tabelele existente
    /// Users/Profiles/RefreshTokens (database-first style — NU generăm tabele noi).
    /// </summary>
    public class StreamZoneDbContext : DbContext
    {
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<UserProfile> Profiles => Set<UserProfile>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public StreamZoneDbContext(DbContextOptions<StreamZoneDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ── Users ────────────────────────────────────────────────────────
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(a => a.UserId);
                entity.Property(a => a.UserId).HasColumnName("UserId").ValueGeneratedOnAdd();
                entity.Property(a => a.Email).HasColumnName("Email").HasMaxLength(256).IsRequired();
                entity.Property(a => a.Username).HasColumnName("Username").HasMaxLength(50).IsRequired();
                entity.Property(a => a.PasswordHash).HasColumnName("PasswordHash").HasColumnType("varbinary(256)").IsRequired();
                entity.Property(a => a.PasswordSalt).HasColumnName("PasswordSalt").HasColumnType("varbinary(128)").IsRequired();
                entity.Property(a => a.EmailConfirmed).HasColumnName("EmailConfirmed");
                entity.Property(a => a.IsActive).HasColumnName("IsActive");
                entity.Property(a => a.CreatedAt).HasColumnName("CreatedAt");
                entity.Property(a => a.LastLoginAt).HasColumnName("LastLoginAt");
                entity.Property(a => a.FailedLoginAttempts).HasColumnName("FailedLoginAttempts");
                entity.Property(a => a.LockoutEndUtc).HasColumnName("LockoutEndUtc");

                entity.HasIndex(a => a.Email).IsUnique();
                entity.HasIndex(a => a.Username).IsUnique();
            });

            // ── Profiles ─────────────────────────────────────────────────────
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.ToTable("Profiles");
                entity.HasKey(p => p.ProfileId);
                entity.Property(p => p.ProfileId).HasColumnName("ProfileId").ValueGeneratedOnAdd();
                entity.Property(p => p.UserId).HasColumnName("UserId");
                entity.Property(p => p.DisplayName).HasColumnName("DisplayName").HasMaxLength(50).IsRequired();
                entity.Property(p => p.AvatarUrl).HasColumnName("AvatarUrl").HasMaxLength(500);
                entity.Property(p => p.IsKidsProfile).HasColumnName("IsKidsProfile");
                entity.Property(p => p.PreferredLanguage).HasColumnName("PreferredLanguage").HasMaxLength(10);
                entity.Property(p => p.CreatedAt).HasColumnName("CreatedAt");

                entity.HasOne(p => p.Account)
                      .WithMany(a => a.Profiles)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── RefreshTokens ────────────────────────────────────────────────
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");
                entity.HasKey(t => t.TokenId);
                entity.Property(t => t.TokenId).HasColumnName("TokenId").ValueGeneratedOnAdd();
                entity.Property(t => t.UserId).HasColumnName("UserId");
                entity.Property(t => t.Token).HasColumnName("Token").HasMaxLength(512).IsRequired();
                entity.Property(t => t.ExpiresAt).HasColumnName("ExpiresAt");
                entity.Property(t => t.CreatedAt).HasColumnName("CreatedAt");
                entity.Property(t => t.RevokedAt).HasColumnName("RevokedAt");

                entity.HasIndex(t => t.Token).IsUnique();

                entity.HasOne(t => t.Account)
                      .WithMany(a => a.RefreshTokens)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
