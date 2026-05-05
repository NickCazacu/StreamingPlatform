using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StreamingPlatform.Auth;

namespace StreamingPlatform.Api
{
    /// <summary>
    /// Endpoint-uri pentru conturi (Users) și profile de vizionare (Profiles).
    /// Persistate în SQL Server (StreamZoneDB) prin EF Core.
    /// </summary>
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(WebApplication app)
        {
            // ── AUTH: register ───────────────────────────────────────────────
            app.MapPost("/api/auth/register", async (HttpContext ctx, AuthService auth) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<RegisterRequest>();
                if (body == null)
                    return Results.BadRequest(new { error = "Body invalid." });

                var result = await auth.RegisterAsync(body.Email, body.Username, body.Password);
                if (!result.Success)
                    return Results.BadRequest(new { error = result.Message });

                return Results.Ok(new
                {
                    message = result.Message,
                    userId = result.UserId,
                    username = result.Username,
                    email = result.Email
                });
            });

            // ── AUTH: login ──────────────────────────────────────────────────
            app.MapPost("/api/auth/login", async (HttpContext ctx, AuthService auth, StreamZoneDbContext db) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<LoginRequest>();
                if (body == null)
                    return Results.BadRequest(new { error = "Body invalid." });

                var result = await auth.LoginAsync(body.EmailOrUsername, body.Password);
                if (!result.Success || result.UserId == null)
                    return Results.Json(new { error = result.Message }, statusCode: 401);

                var profiles = await db.Profiles
                    .Where(p => p.UserId == result.UserId)
                    .Select(p => new
                    {
                        profileId = p.ProfileId,
                        displayName = p.DisplayName,
                        avatarUrl = p.AvatarUrl,
                        isKidsProfile = p.IsKidsProfile,
                        preferredLanguage = p.PreferredLanguage
                    })
                    .ToListAsync();

                return Results.Ok(new
                {
                    message = result.Message,
                    userId = result.UserId,
                    username = result.Username,
                    email = result.Email,
                    accessToken = result.AccessToken,
                    refreshToken = result.RefreshToken,
                    accessTokenExpiresAt = result.AccessTokenExpiresAt,
                    profiles
                });
            });

            // ── AUTH: refresh ────────────────────────────────────────────────
            app.MapPost("/api/auth/refresh", async (HttpContext ctx, AuthService auth) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<RefreshRequest>();
                if (body == null || string.IsNullOrWhiteSpace(body.RefreshToken))
                    return Results.BadRequest(new { error = "RefreshToken lipsă." });

                var result = await auth.RefreshAsync(body.RefreshToken);
                if (!result.Success)
                    return Results.Json(new { error = result.Message }, statusCode: 401);

                return Results.Ok(new
                {
                    accessToken = result.AccessToken,
                    refreshToken = result.RefreshToken,
                    accessTokenExpiresAt = result.AccessTokenExpiresAt,
                    userId = result.UserId,
                    username = result.Username
                });
            });

            // ── AUTH: logout ─────────────────────────────────────────────────
            app.MapPost("/api/auth/logout", async (HttpContext ctx, AuthService auth) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<RefreshRequest>();
                if (body == null || string.IsNullOrWhiteSpace(body.RefreshToken))
                    return Results.BadRequest(new { error = "RefreshToken lipsă." });

                var ok = await auth.LogoutAsync(body.RefreshToken);
                return Results.Ok(new { success = ok, message = ok ? "Logout reușit." : "Token deja revocat sau inexistent." });
            });

            // ── PROFILES: list ───────────────────────────────────────────────
            app.MapGet("/api/accounts/{userId:int}/profiles", async (int userId, StreamZoneDbContext db) =>
            {
                var account = await db.Accounts.FindAsync(userId);
                if (account == null)
                    return Results.NotFound(new { error = $"Contul {userId} nu există." });

                var profiles = await db.Profiles
                    .Where(p => p.UserId == userId)
                    .OrderBy(p => p.CreatedAt)
                    .Select(p => new
                    {
                        profileId = p.ProfileId,
                        displayName = p.DisplayName,
                        avatarUrl = p.AvatarUrl,
                        isKidsProfile = p.IsKidsProfile,
                        preferredLanguage = p.PreferredLanguage,
                        createdAt = p.CreatedAt
                    })
                    .ToListAsync();

                return Results.Ok(new { userId, count = profiles.Count, profiles });
            });

            // ── PROFILES: create ─────────────────────────────────────────────
            app.MapPost("/api/accounts/{userId:int}/profiles", async (int userId, HttpContext ctx, StreamZoneDbContext db) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<CreateProfileRequest>();
                if (body == null || string.IsNullOrWhiteSpace(body.DisplayName))
                    return Results.BadRequest(new { error = "DisplayName este obligatoriu." });

                var account = await db.Accounts.FindAsync(userId);
                if (account == null)
                    return Results.NotFound(new { error = $"Contul {userId} nu există." });

                var profileCount = await db.Profiles.CountAsync(p => p.UserId == userId);
                if (profileCount >= 5)
                    return Results.BadRequest(new { error = "Limit atins: maxim 5 profile per cont." });

                var profile = new UserProfile
                {
                    UserId = userId,
                    DisplayName = body.DisplayName.Trim(),
                    AvatarUrl = body.AvatarUrl,
                    IsKidsProfile = body.IsKidsProfile ?? false,
                    PreferredLanguage = string.IsNullOrWhiteSpace(body.PreferredLanguage) ? "ro" : body.PreferredLanguage!,
                    CreatedAt = DateTime.UtcNow
                };
                db.Profiles.Add(profile);
                await db.SaveChangesAsync();

                return Results.Ok(new
                {
                    message = $"Profilul '{profile.DisplayName}' a fost creat.",
                    profile = new
                    {
                        profileId = profile.ProfileId,
                        displayName = profile.DisplayName,
                        avatarUrl = profile.AvatarUrl,
                        isKidsProfile = profile.IsKidsProfile,
                        preferredLanguage = profile.PreferredLanguage
                    }
                });
            });

            // ── PROFILES: update ─────────────────────────────────────────────
            app.MapPut("/api/profiles/{profileId:int}", async (int profileId, HttpContext ctx, StreamZoneDbContext db) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<UpdateProfileRequest>();
                if (body == null) return Results.BadRequest(new { error = "Body invalid." });

                var profile = await db.Profiles.FindAsync(profileId);
                if (profile == null) return Results.NotFound(new { error = $"Profilul {profileId} nu există." });

                if (!string.IsNullOrWhiteSpace(body.DisplayName)) profile.DisplayName = body.DisplayName.Trim();
                if (body.AvatarUrl != null) profile.AvatarUrl = body.AvatarUrl;
                if (body.IsKidsProfile.HasValue) profile.IsKidsProfile = body.IsKidsProfile.Value;
                if (!string.IsNullOrWhiteSpace(body.PreferredLanguage)) profile.PreferredLanguage = body.PreferredLanguage!;

                await db.SaveChangesAsync();
                return Results.Ok(new { message = "Profil actualizat.", profileId, displayName = profile.DisplayName });
            });

            // ── PROFILES: delete ─────────────────────────────────────────────
            app.MapDelete("/api/profiles/{profileId:int}", async (int profileId, StreamZoneDbContext db) =>
            {
                var profile = await db.Profiles.FindAsync(profileId);
                if (profile == null) return Results.NotFound(new { error = $"Profilul {profileId} nu există." });

                db.Profiles.Remove(profile);
                await db.SaveChangesAsync();
                return Results.Ok(new { message = $"Profilul '{profile.DisplayName}' a fost șters." });
            });

            // ── DB HEALTH ────────────────────────────────────────────────────
            app.MapGet("/api/db/health", async (StreamZoneDbContext db) =>
            {
                try
                {
                    var canConnect = await db.Database.CanConnectAsync();
                    var userCount = canConnect ? await db.Accounts.CountAsync() : 0;
                    var profileCount = canConnect ? await db.Profiles.CountAsync() : 0;
                    return Results.Ok(new
                    {
                        connected = canConnect,
                        database = "StreamZoneDB",
                        accounts = userCount,
                        profiles = profileCount
                    });
                }
                catch (Exception ex)
                {
                    return Results.Json(new { connected = false, error = ex.Message }, statusCode: 500);
                }
            });
        }

        // ── Request DTOs ─────────────────────────────────────────────────────
        private record RegisterRequest(string Email, string Username, string Password);
        private record LoginRequest(string EmailOrUsername, string Password);
        private record RefreshRequest(string RefreshToken);
        private record CreateProfileRequest(string DisplayName, string? AvatarUrl, bool? IsKidsProfile, string? PreferredLanguage);
        private record UpdateProfileRequest(string? DisplayName, string? AvatarUrl, bool? IsKidsProfile, string? PreferredLanguage);
    }
}
