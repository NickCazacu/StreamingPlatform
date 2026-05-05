using System;
using System.Collections.Generic;
using StreamingPlatform.Models;
using StreamingPlatform.Services;

namespace StreamingPlatform.Proxy
{
    /// <summary>
    /// PROTECTION PROXY — Se interpune între client și RealContentPlayer.
    ///
    /// Implementează aceeași interfață IContentPlayer ca subiectul real,
    /// deci clientul nu observă diferența.
    ///
    /// Pași la fiecare Play():
    ///   1. Autentificare — utilizatorul există?
    ///   2. Abonament    — tipul abonamentului permite acest rating?
    ///   3. Vârstă       — utilizatorul are vârsta minimă necesară?
    ///   4. Logging      — înregistrăm accesul (permis sau refuzat)
    ///   5. Delegare     — dacă totul e OK, delegăm la RealContentPlayer
    /// </summary>
    public class ContentAccessProxy : IContentPlayer
    {
        private readonly RealContentPlayer _realPlayer;
        private readonly Dictionary<string, UserProfile> _userRegistry;
        private readonly List<string> _accessLog = new();

        public ContentAccessProxy(MediaContent content,
            Dictionary<string, UserProfile> userRegistry)
        {
            _realPlayer   = new RealContentPlayer(content);
            _userRegistry = userRegistry ?? throw new ArgumentNullException(nameof(userRegistry));
        }

        public string GetTitle() => _realPlayer.GetTitle();

        /// <summary>
        /// GetInfo e accesibil FĂRĂ autentificare — e doar o previzualizare.
        /// </summary>
        public string GetInfo()
        {
            return _realPlayer.GetInfo() +
                   "\n   [Proxy] Autentifică-te și abonează-te pentru a viziona.";
        }

        /// <summary>
        /// Verifică accesul fără a reda — util pentru UI (buton Play gri/activ).
        /// </summary>
        public bool CanUserAccess(string userName)
        {
            if (!_userRegistry.TryGetValue(userName, out UserProfile? profile))
                return false;
            ContentRating rating = _realPlayer.GetRating();
            return CheckSubscription(profile.Subscription, rating)
                && CheckAge(profile.Age, rating);
        }

        /// <summary>
        /// METODA PRINCIPALĂ — Proxy interceptează și decide.
        /// ACCES PERMIS  → deleghează la RealContentPlayer.Play()
        /// ACCES REFUZAT → returnează mesaj de eroare fără să apeleze realul
        /// </summary>
        public string Play(string userName)
        {
            // ── Pas 1: Autentificare ─────────────────────────────────────────
            if (!_userRegistry.TryGetValue(userName, out UserProfile? profile))
                return LogAndReturn($"ACCES REFUZAT: '{userName}' nu este autentificat.", denied: true);

            ContentRating rating = _realPlayer.GetRating();

            // ── Pas 2: Verificare abonament ───────────────────────────────────
            if (!CheckSubscription(profile.Subscription, rating))
            {
                string reason = $"Abonament {profile.Subscription} nu permite conținut {rating}";
                return LogAndReturn(
                    $"ACCES REFUZAT: '{userName}' — {reason}. Actualizează la Premium!",
                    denied: true);
            }

            // ── Pas 3: Verificare vârstă ──────────────────────────────────────
            if (!CheckAge(profile.Age, rating))
            {
                int required = GetRequiredAge(rating);
                return LogAndReturn(
                    $"ACCES REFUZAT: '{userName}' ({profile.Age} ani) sub limita {rating} ({required}+ ani).",
                    denied: true);
            }

            // ── Pas 4: Acces permis — logăm și delegăm ────────────────────────
            string allowMsg = $"ACCES PERMIS: '{userName}' ({profile.Subscription}, {profile.Age} ani) → '{GetTitle()}'";
            Log(allowMsg);
            Console.WriteLine($"      [Proxy] {allowMsg}");

            return _realPlayer.Play(userName);
        }

        public IReadOnlyList<string> GetAccessLog() => _accessLog.AsReadOnly();

        public string GetAccessReport()
        {
            if (_accessLog.Count == 0) return "   Nicio accesare înregistrată.";
            string report = $"   Log acces pentru '{GetTitle()}' ({_accessLog.Count} intrări):\n";
            foreach (string entry in _accessLog)
                report += $"   {entry}\n";
            return report;
        }

        private static bool CheckSubscription(SubscriptionType sub, ContentRating rating) =>
            sub switch
            {
                SubscriptionType.Premium  => true,
                SubscriptionType.Standard => rating != ContentRating.R,
                SubscriptionType.Free     => rating is ContentRating.G or ContentRating.PG,
                _                         => false
            };

        private static bool CheckAge(int age, ContentRating rating) =>
            rating switch
            {
                ContentRating.R    => age >= 17,
                ContentRating.PG13 => age >= 13,
                ContentRating.PG   => age >= 7,
                ContentRating.G    => true,
                _                  => false
            };

        private static int GetRequiredAge(ContentRating rating) =>
            rating switch
            {
                ContentRating.R    => 17,
                ContentRating.PG13 => 13,
                ContentRating.PG   => 7,
                _                  => 0
            };

        private string LogAndReturn(string message, bool denied)
        {
            Log(message);
            return denied ? $"❌ [Proxy] {message}" : $"✅ [Proxy] {message}";
        }

        private void Log(string message)
        {
            string entry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            _accessLog.Add(entry);
            PlatformManager.Instance.Log($"[Proxy] {message}");
        }
    }
}
