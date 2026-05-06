using Microsoft.EntityFrameworkCore;
using StreamingPlatform.Models.Db;

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
        public DbSet<WatchEvent> WatchEvents => Set<WatchEvent>();

        // ── Conținut media (database-first: tabele create manual via SQL) ────
        public DbSet<MovieEntity> Movies => Set<MovieEntity>();
        public DbSet<SeriesEntity> SeriesItems => Set<SeriesEntity>();
        public DbSet<DocumentaryEntity> Documentaries => Set<DocumentaryEntity>();

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
                entity.Property(a => a.PhoneNumber).HasColumnName("PhoneNumber").HasMaxLength(20);
                entity.Property(a => a.SubscriptionType).HasColumnName("SubscriptionType").HasMaxLength(20).IsRequired();
                entity.Property(a => a.SubscriptionExpiresAt).HasColumnName("SubscriptionExpiresAt");

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

            // ── Movies ───────────────────────────────────────────────────────
            modelBuilder.Entity<MovieEntity>(entity =>
            {
                entity.ToTable("Movies");
                entity.HasKey(m => m.MovieId);
                entity.Property(m => m.MovieId).HasColumnName("MovieId").ValueGeneratedOnAdd();
                entity.Property(m => m.Title).HasColumnName("Title").HasMaxLength(200).IsRequired();
                entity.Property(m => m.Description).HasColumnName("Description").HasMaxLength(1000).IsRequired();
                entity.Property(m => m.Genre).HasColumnName("Genre").HasMaxLength(50).IsRequired();
                entity.Property(m => m.ContentRating).HasColumnName("ContentRating").HasMaxLength(10).IsRequired();
                entity.Property(m => m.DurationMinutes).HasColumnName("DurationMinutes");
                entity.Property(m => m.Director).HasColumnName("Director").HasMaxLength(150).IsRequired();
                entity.Property(m => m.AverageRating).HasColumnName("AverageRating").HasColumnType("decimal(3,2)");
                entity.Property(m => m.IsMoldovan).HasColumnName("IsMoldovan");
                entity.Property(m => m.PosterUrl).HasColumnName("PosterUrl").HasMaxLength(500);
                entity.Property(m => m.CreatedAt).HasColumnName("CreatedAt");
            });

            // ── Series ───────────────────────────────────────────────────────
            modelBuilder.Entity<SeriesEntity>(entity =>
            {
                entity.ToTable("Series");
                entity.HasKey(s => s.SeriesId);
                entity.Property(s => s.SeriesId).HasColumnName("SeriesId").ValueGeneratedOnAdd();
                entity.Property(s => s.Title).HasColumnName("Title").HasMaxLength(200).IsRequired();
                entity.Property(s => s.Description).HasColumnName("Description").HasMaxLength(1000).IsRequired();
                entity.Property(s => s.Genre).HasColumnName("Genre").HasMaxLength(50).IsRequired();
                entity.Property(s => s.ContentRating).HasColumnName("ContentRating").HasMaxLength(10).IsRequired();
                entity.Property(s => s.Creator).HasColumnName("Creator").HasMaxLength(150).IsRequired();
                entity.Property(s => s.SeasonsCount).HasColumnName("SeasonsCount");
                entity.Property(s => s.EpisodesCount).HasColumnName("EpisodesCount");
                entity.Property(s => s.EpisodeDuration).HasColumnName("EpisodeDuration");
                entity.Property(s => s.IsCompleted).HasColumnName("IsCompleted");
                entity.Property(s => s.AverageRating).HasColumnName("AverageRating").HasColumnType("decimal(3,2)");
                entity.Property(s => s.IsMoldovan).HasColumnName("IsMoldovan");
                entity.Property(s => s.PosterUrl).HasColumnName("PosterUrl").HasMaxLength(500);
                entity.Property(s => s.CreatedAt).HasColumnName("CreatedAt");
            });

            // ── Documentaries ────────────────────────────────────────────────
            modelBuilder.Entity<DocumentaryEntity>(entity =>
            {
                entity.ToTable("Documentaries");
                entity.HasKey(d => d.DocumentaryId);
                entity.Property(d => d.DocumentaryId).HasColumnName("DocumentaryId").ValueGeneratedOnAdd();
                entity.Property(d => d.Title).HasColumnName("Title").HasMaxLength(200).IsRequired();
                entity.Property(d => d.Description).HasColumnName("Description").HasMaxLength(1000).IsRequired();
                entity.Property(d => d.Genre).HasColumnName("Genre").HasMaxLength(50).IsRequired();
                entity.Property(d => d.ContentRating).HasColumnName("ContentRating").HasMaxLength(10).IsRequired();
                entity.Property(d => d.DurationMinutes).HasColumnName("DurationMinutes");
                entity.Property(d => d.Topic).HasColumnName("Topic").HasMaxLength(100).IsRequired();
                entity.Property(d => d.Narrator).HasColumnName("Narrator").HasMaxLength(150).IsRequired();
                entity.Property(d => d.IsEducational).HasColumnName("IsEducational");
                entity.Property(d => d.AverageRating).HasColumnName("AverageRating").HasColumnType("decimal(3,2)");
                entity.Property(d => d.IsMoldovan).HasColumnName("IsMoldovan");
                entity.Property(d => d.PosterUrl).HasColumnName("PosterUrl").HasMaxLength(500);
                entity.Property(d => d.CreatedAt).HasColumnName("CreatedAt");
            });

            // ── WatchEvents ──────────────────────────────────────────────────
            modelBuilder.Entity<WatchEvent>(entity =>
            {
                entity.ToTable("WatchEvents");
                entity.HasKey(w => w.WatchEventId);
                entity.Property(w => w.WatchEventId).HasColumnName("WatchEventId").ValueGeneratedOnAdd();
                entity.Property(w => w.UserId).HasColumnName("UserId");
                entity.Property(w => w.ContentTitle).HasColumnName("ContentTitle").HasMaxLength(200).IsRequired();
                entity.Property(w => w.ContentType).HasColumnName("ContentType").HasMaxLength(20).IsRequired();
                entity.Property(w => w.Quality).HasColumnName("Quality").HasMaxLength(10);
                entity.Property(w => w.WatchedAt).HasColumnName("WatchedAt");

                entity.HasOne(w => w.Account)
                      .WithMany()
                      .HasForeignKey(w => w.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(w => new { w.UserId, w.WatchedAt });
            });
        }
    }
}
