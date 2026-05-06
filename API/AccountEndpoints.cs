using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StreamingPlatform.Auth;
using StreamingPlatform.Models.Db;
using StreamingPlatform.Services;

namespace StreamingPlatform.Api
{
    /// <summary>
    /// Endpoint-uri pentru gestiunea contului utilizatorului:
    /// - Telefon (pentru notificări SMS — Decorator pattern)
    /// - Abonament Premium (cu limite zilnice și calitate maximă)
    /// - Tracking vizionări pentru limitele Standard / Free
    /// </summary>
    public static class AccountEndpoints
    {
        // ── Reguli de tier ────────────────────────────────────────────────
        // Free:     1 film/zi, 2 episoade/zi, max 1080p
        // Premium:  fără limite, calitate maximă (4K)
        private record TierRules(int MoviesPerDay, int EpisodesPerDay, string MaxQuality);

        private static TierRules GetRules(string sub) => sub switch
        {
            "Premium" => new TierRules(int.MaxValue, int.MaxValue, "4K"),
            _         => new TierRules(1, 2, "1080p")     // Free (și orice tier vechi mapat aici)
        };

        private static readonly Regex PhoneRegex = new(@"^\+?[0-9\s\-]{7,20}$", RegexOptions.Compiled);

        public static void MapAccountEndpoints(WebApplication app)
        {
            // ── GET /api/account/me?userId=X ─────────────────────────────
            // Întoarce profilul complet: tier, telefon, expirare, vizionări de azi
            app.MapGet("/api/account/me", async (int userId, StreamZoneDbContext db) =>
            {
                var account = await db.Accounts.FindAsync(userId);
                if (account == null)
                    return Results.NotFound(new { error = $"Cont {userId} inexistent." });

                var (movies, episodes) = await CountTodayAsync(db, userId);
                var rules = GetRules(account.SubscriptionType);

                return Results.Ok(new
                {
                    userId = account.UserId,
                    username = account.Username,
                    email = account.Email,
                    phoneNumber = account.PhoneNumber,
                    subscription = new
                    {
                        type = account.SubscriptionType,
                        expiresAt = account.SubscriptionExpiresAt,
                        isActive = account.SubscriptionType == "Premium"
                                   && account.SubscriptionExpiresAt.HasValue
                                   && account.SubscriptionExpiresAt.Value > DateTime.UtcNow
                    },
                    limits = new
                    {
                        moviesPerDay   = rules.MoviesPerDay   == int.MaxValue ? (int?)null : rules.MoviesPerDay,
                        episodesPerDay = rules.EpisodesPerDay == int.MaxValue ? (int?)null : rules.EpisodesPerDay,
                        maxQuality     = rules.MaxQuality
                    },
                    todayUsage = new
                    {
                        movies = movies,
                        episodes = episodes
                    }
                });
            });

            // ── PUT /api/account/{userId}/phone ──────────────────────────
            app.MapPut("/api/account/{userId:int}/phone", async (int userId, HttpContext ctx, StreamZoneDbContext db) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<PhoneRequest>();
                if (body == null)
                    return Results.BadRequest(new { error = "Body invalid." });

                var account = await db.Accounts.FindAsync(userId);
                if (account == null)
                    return Results.NotFound(new { error = $"Cont {userId} inexistent." });

                var phone = body.PhoneNumber?.Trim();
                if (string.IsNullOrWhiteSpace(phone))
                {
                    account.PhoneNumber = null;
                    await db.SaveChangesAsync();
                    return Results.Ok(new { message = "Telefon șters." });
                }

                if (!PhoneRegex.IsMatch(phone))
                    return Results.BadRequest(new { error = "Format invalid. Folosește: +37368xxxxxx" });

                account.PhoneNumber = phone;
                await db.SaveChangesAsync();
                PlatformManager.Instance.Log($"[Account] Telefon actualizat pentru {account.Username}: {phone}");
                return Results.Ok(new { message = "Telefon salvat. SMS-urile vor fi trimise aici.", phoneNumber = phone });
            });

            // ── POST /api/account/{userId}/subscribe ────────────────────
            // Mock payment — schimbă tier-ul
            app.MapPost("/api/account/{userId:int}/subscribe", async (int userId, HttpContext ctx, StreamZoneDbContext db) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<SubscribeRequest>();
                if (body == null || string.IsNullOrWhiteSpace(body.PlanType))
                    return Results.BadRequest(new { error = "PlanType obligatoriu (Free | Premium)." });

                var validPlans = new[] { "Free", "Premium" };
                if (!validPlans.Contains(body.PlanType))
                    return Results.BadRequest(new { error = $"PlanType invalid. Valori: {string.Join(", ", validPlans)}" });

                var account = await db.Accounts.FindAsync(userId);
                if (account == null)
                    return Results.NotFound(new { error = $"Cont {userId} inexistent." });

                // Mock validare card — doar pentru planurile plătite (Premium)
                if (body.PlanType == "Premium")
                {
                    var cardDigits = (body.CardNumber ?? "").Replace(" ", "").Replace("-", "");
                    if (cardDigits.Length != 16 || !cardDigits.All(char.IsDigit))
                        return Results.BadRequest(new { error = "Numărul cardului trebuie să aibă 16 cifre." });
                }

                account.SubscriptionType = body.PlanType;
                account.SubscriptionExpiresAt = body.PlanType == "Free"
                    ? null
                    : DateTime.UtcNow.AddDays(30);

                await db.SaveChangesAsync();
                PlatformManager.Instance.Log($"[Account] {account.Username} → {body.PlanType}");

                string message = body.PlanType switch
                {
                    "Free" => "Abonament anulat. Ai revenit la planul Free.",
                    _      => "Abonament Premium activat. Plată: 199 MDL/lună"
                };

                return Results.Ok(new
                {
                    message,
                    subscription = new
                    {
                        type = account.SubscriptionType,
                        expiresAt = account.SubscriptionExpiresAt
                    }
                });
            });

            // ── POST /api/account/{userId}/play ──────────────────────────
            // Verifică limita ÎNAINTE de play; dacă e OK, înregistrează vizionarea.
            app.MapPost("/api/account/{userId:int}/play", async (int userId, HttpContext ctx, StreamZoneDbContext db) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<PlayRequest>();
                if (body == null || string.IsNullOrWhiteSpace(body.Title))
                    return Results.BadRequest(new { error = "Title obligatoriu." });

                var account = await db.Accounts.FindAsync(userId);
                if (account == null)
                    return Results.NotFound(new { error = $"Cont {userId} inexistent." });

                // Determină tipul conținutului din BD
                string contentType = "Movie";
                bool foundInDb = false;
                if (await db.Movies.AnyAsync(m => m.Title == body.Title))         { contentType = "Movie"; foundInDb = true; }
                else if (await db.SeriesItems.AnyAsync(s => s.Title == body.Title)) { contentType = "Series"; foundInDb = true; }
                else if (await db.Documentaries.AnyAsync(d => d.Title == body.Title)) { contentType = "Documentary"; foundInDb = true; }

                var rules = GetRules(account.SubscriptionType);
                var (todayMovies, todayEpisodes) = await CountTodayAsync(db, userId);

                // ── Verificare limită ─────────────────────────────────────
                bool allowed = true;
                string? reason = null;

                if (contentType == "Movie" && todayMovies >= rules.MoviesPerDay)
                {
                    allowed = false;
                    reason = account.SubscriptionType == "Free"
                        ? "Planul Free nu permite vizionarea de filme. Upgrade la Standard sau Premium."
                        : $"Ai atins limita de {rules.MoviesPerDay} film/zi pentru planul {account.SubscriptionType}. Upgrade la Premium pentru vizionări nelimitate.";
                }
                else if (contentType == "Series" && todayEpisodes >= rules.EpisodesPerDay)
                {
                    allowed = false;
                    reason = $"Ai atins limita de {rules.EpisodesPerDay} episoade/zi pentru planul {account.SubscriptionType}. Upgrade la Premium pentru vizionări nelimitate.";
                }

                // ── Verificare calitate maximă ────────────────────────────
                string requestedQuality = body.Quality ?? "720p";
                string effectiveQuality = ClampQuality(requestedQuality, rules.MaxQuality);
                bool qualityClamped = effectiveQuality != requestedQuality;

                // ── Înregistrăm doar dacă e permis ────────────────────────
                if (allowed)
                {
                    db.WatchEvents.Add(new WatchEvent
                    {
                        UserId = userId,
                        ContentTitle = body.Title,
                        ContentType = contentType,
                        Quality = effectiveQuality,
                        WatchedAt = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync();

                    if (contentType == "Movie") todayMovies++;
                    else if (contentType == "Series") todayEpisodes++;
                }

                return Results.Ok(new
                {
                    allowed,
                    reason,
                    contentType,
                    foundInDb,
                    quality = new
                    {
                        requested = requestedQuality,
                        effective = effectiveQuality,
                        clamped = qualityClamped,
                        max = rules.MaxQuality
                    },
                    subscription = account.SubscriptionType,
                    todayUsage = new
                    {
                        movies = todayMovies,
                        episodes = todayEpisodes,
                        moviesLimit   = rules.MoviesPerDay   == int.MaxValue ? (int?)null : rules.MoviesPerDay,
                        episodesLimit = rules.EpisodesPerDay == int.MaxValue ? (int?)null : rules.EpisodesPerDay
                    }
                });
            });
        }

        private static async Task<(int movies, int episodes)> CountTodayAsync(StreamZoneDbContext db, int userId)
        {
            var startOfDay = DateTime.UtcNow.Date;
            var todayEvents = await db.WatchEvents
                .Where(w => w.UserId == userId && w.WatchedAt >= startOfDay)
                .ToListAsync();

            int movies = todayEvents.Count(e => e.ContentType == "Movie");
            int episodes = todayEvents.Count(e => e.ContentType == "Series");
            return (movies, episodes);
        }

        private static string ClampQuality(string requested, string max)
        {
            int Rank(string q) => q switch
            {
                "4K" or "4KUHD" => 4,
                "1080p" => 3,
                "720p"  => 2,
                "480p"  => 1,
                _       => 0
            };
            return Rank(requested) > Rank(max) ? max : requested;
        }

        private record PhoneRequest(string? PhoneNumber);
        private record SubscribeRequest(string PlanType, string? CardNumber, string? CardHolder);
        private record PlayRequest(string Title, string? Quality);
    }
}
