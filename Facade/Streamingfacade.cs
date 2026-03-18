using System;
using System.Collections.Generic;
using System.Linq;
using StreamingPlatform.Models;
using StreamingPlatform.Interfaces;
using StreamingPlatform.Adapters;
using StreamingPlatform.Composite;
using StreamingPlatform.Services;

namespace StreamingPlatform.Facade
{
    // ============================================================
    // FAÇADE PATTERN
    // ============================================================
    // Problema: Pentru a viziona un film pe platformă, clientul trebuie:
    // 1. Să verifice dacă utilizatorul există
    // 2. Să verifice abonamentul
    // 3. Să verifice restricția de vârstă
    // 4. Să caute conținutul
    // 5. Să obțină rating-uri externe
    // 6. Să pornească redarea
    // 7. Să logheze acțiunea
    // 8. Să actualizeze statisticile
    //
    // Fiecare pas implică un serviciu diferit — e complex!
    //
    // Soluția: StreamingFacade oferă METODE SIMPLE care fac totul intern.
    // Clientul scrie: facade.WatchContent("John", "Inception")
    // Fațada se ocupă de toate cele 8 pași.
    // ============================================================

    /// <summary>
    /// Subsistem intern: Verificarea abonamentelor.
    /// </summary>
    public class SubscriptionChecker
    {
        public bool HasActiveSubscription(User user)
        {
            return user.Subscription.Type != SubscriptionType.Free ||
                   user.Subscription.IsActive;
        }

        public bool CanAccessContent(User user, MediaContent content)
        {
            // Premium vede tot, Standard nu vede R-rated, Free doar PG
            return user.Subscription.Type switch
            {
                SubscriptionType.Premium => true,
                SubscriptionType.Standard => content.Rating != ContentRating.R,
                SubscriptionType.Free => content.Rating == ContentRating.PG ||
                                         content.Rating == ContentRating.G,
                _ => false
            };
        }

        public string GetAccessLevel(User user)
        {
            return user.Subscription.Type switch
            {
                SubscriptionType.Premium => "Acces complet (toate conținuturile)",
                SubscriptionType.Standard => "Acces standard (fără conținut R-rated)",
                SubscriptionType.Free => "Acces limitat (doar PG și G)",
                _ => "Necunoscut"
            };
        }
    }

    /// <summary>
    /// Subsistem intern: Verificarea restricțiilor de vârstă.
    /// </summary>
    public class AgeVerifier
    {
        public bool CanWatch(User user, MediaContent content)
        {
            int requiredAge = content.Rating switch
            {
                ContentRating.G => 0,
                ContentRating.PG => 7,
                ContentRating.PG13 => 13,
                ContentRating.R => 17,
                _ => 0
            };
            return user.Age >= requiredAge;
        }

        public string GetRestrictionMessage(User user, MediaContent content)
        {
            if (CanWatch(user, content)) return "OK";

            int requiredAge = content.Rating switch
            {
                ContentRating.PG13 => 13,
                ContentRating.R => 17,
                _ => 0
            };
            return $"Vârstă insuficientă: necesari {requiredAge} ani, utilizatorul are {user.Age}";
        }
    }

    /// <summary>
    /// Subsistem intern: Motor de căutare conținut.
    /// </summary>
    public class ContentSearchEngine
    {
        private readonly List<MediaContent> _contentLibrary = new List<MediaContent>();

        public void AddToLibrary(MediaContent content)
        {
            _contentLibrary.Add(content);
        }

        public void AddRangeToLibrary(IEnumerable<MediaContent> contents)
        {
            _contentLibrary.AddRange(contents);
        }

        public MediaContent FindByTitle(string title)
        {
            return _contentLibrary.FirstOrDefault(c =>
                c.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        }

        public List<MediaContent> SearchByGenre(Genre genre)
        {
            return _contentLibrary.Where(c => c.Genre == genre).ToList();
        }

        public List<MediaContent> SearchByKeyword(string keyword)
        {
            return _contentLibrary.Where(c =>
                c.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                c.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<MediaContent> GetTopRated(int count = 5)
        {
            return _contentLibrary
                .OrderByDescending(c => c.AverageRating)
                .Take(count)
                .ToList();
        }
    }

    /// <summary>
    /// Subsistem intern: Sistem de notificări.
    /// </summary>
    public class NotificationService
    {
        private readonly List<string> _notifications = new List<string>();

        public void SendNotification(string userName, string message)
        {
            string notification = $"[{DateTime.Now:HH:mm:ss}] → {userName}: {message}";
            _notifications.Add(notification);
            Console.WriteLine($"    📧 Notificare trimisă: {message}");
        }

        public IReadOnlyList<string> GetNotifications() => _notifications.AsReadOnly();
        public int GetCount() => _notifications.Count;
    }

    // ============================================================
    // FAȚADA — Interfață simplificată
    // ============================================================
    public class StreamingFacade
    {
        // Subsisteme interne (complexitatea ascunsă)
        private readonly SubscriptionChecker _subscriptionChecker;
        private readonly AgeVerifier _ageVerifier;
        private readonly ContentSearchEngine _searchEngine;
        private readonly RatingAggregator _ratingAggregator;
        private readonly NotificationService _notificationService;
        private readonly Dictionary<string, User> _users;

        public StreamingFacade()
        {
            _subscriptionChecker = new SubscriptionChecker();
            _ageVerifier = new AgeVerifier();
            _searchEngine = new ContentSearchEngine();
            _ratingAggregator = new RatingAggregator();
            _notificationService = new NotificationService();
            _users = new Dictionary<string, User>();

            // Adăugăm serviciile de rating externe (folosim Adapter!)
            _ratingAggregator.AddService(new ImdbAdapter(new ImdbService()));
            _ratingAggregator.AddService(new RottenTomatoesAdapter(new RottenTomatoesService()));
            _ratingAggregator.AddService(new MetacriticAdapter(new MetacriticService()));
        }

        // --- Metode simple pentru client ---

        /// <summary>
        /// Înregistrează un utilizator. O singură metodă în loc de
        /// creare user + verificare email + setare abonament + notificare.
        /// </summary>
        public User RegisterUser(string name, string email, int age,
            SubscriptionType subType = SubscriptionType.Free, int months = 1)
        {
            Console.WriteLine($"    Înregistrare utilizator: {name}...");
            var user = new User(name, email, age);

            if (subType != SubscriptionType.Free)
                user.Subscription.Upgrade(subType, months);

            _users[name] = user;

            Console.WriteLine($"    ✅ Utilizator creat: {name} ({subType})");
            _notificationService.SendNotification(name, $"Bun venit pe platformă! Abonament: {subType}");

            // Loghează în Singleton
            PlatformManager.Instance.IncrementUsers();
            PlatformManager.Instance.Log($"Utilizator nou: {name} ({subType})");

            return user;
        }

        /// <summary>
        /// Adaugă conținut în bibliotecă.
        /// </summary>
        public void AddContent(MediaContent content)
        {
            _searchEngine.AddToLibrary(content);
            Console.WriteLine($"    ✅ Conținut adăugat: {content.Title}");
        }

        /// <summary>
        /// METODA PRINCIPALĂ — Vizionare conținut.
        /// O SINGURĂ metodă care face intern 8 pași:
        /// 1) Găsește utilizatorul
        /// 2) Găsește conținutul
        /// 3) Verifică abonamentul
        /// 4) Verifică vârsta
        /// 5) Obține rating-uri externe
        /// 6) Pornește redarea
        /// 7) Loghează
        /// 8) Notifică
        /// </summary>
        public string WatchContent(string userName, string contentTitle)
        {
            Console.WriteLine($"\n    ▶ {userName} vrea să vizioneze '{contentTitle}'...");

            // Pas 1: Găsește utilizatorul
            if (!_users.ContainsKey(userName))
                return $"    ❌ Utilizatorul '{userName}' nu există.";
            var user = _users[userName];

            // Pas 2: Găsește conținutul
            var content = _searchEngine.FindByTitle(contentTitle);
            if (content == null)
                return $"    ❌ Conținutul '{contentTitle}' nu a fost găsit.";

            // Pas 3: Verifică abonamentul
            if (!_subscriptionChecker.CanAccessContent(user, content))
                return $"    ❌ Abonamentul {user.Subscription.Type} nu permite accesul. " +
                       $"Nivel: {_subscriptionChecker.GetAccessLevel(user)}";

            // Pas 4: Verifică vârsta
            if (!_ageVerifier.CanWatch(user, content))
                return $"    ❌ {_ageVerifier.GetRestrictionMessage(user, content)}";

            // Pas 5: Rating-uri externe (prin Adapter!)
            double avgExternal = _ratingAggregator.GetAverageRating(contentTitle);

            // Pas 6: Pornește redarea
            string playResult = content.Play();

            // Pas 7: Loghează în Singleton
            PlatformManager.Instance.IncrementStreams();
            PlatformManager.Instance.Log($"Stream: {userName} → {contentTitle}");

            // Pas 8: Notifică
            _notificationService.SendNotification(userName,
                $"Acum vizionezi: {contentTitle}");

            return $"    ✅ {playResult}\n" +
                   $"    ⭐ Rating extern mediu: {avgExternal}/10";
        }

        /// <summary>
        /// Obține informații complete despre un conținut.
        /// Combină date interne + rating-uri externe (Adapter).
        /// </summary>
        public string GetContentInfo(string contentTitle)
        {
            var content = _searchEngine.FindByTitle(contentTitle);
            if (content == null) return $"Conținut '{contentTitle}' negăsit.";

            string info = content.GetInfo() + "\n";
            info += $"\n   Rating-uri externe:\n";
            info += _ratingAggregator.GetDetailedRatings(contentTitle);
            info += $"   Media externă: {_ratingAggregator.GetAverageRating(contentTitle)}/10\n";
            info += $"\n   Review-uri:\n";
            info += _ratingAggregator.GetAllReviews(contentTitle);

            return info;
        }

        /// <summary>
        /// Caută conținut după gen.
        /// </summary>
        public List<MediaContent> SearchByGenre(Genre genre)
        {
            return _searchEngine.SearchByGenre(genre);
        }

        /// <summary>
        /// Caută conținut după cuvânt cheie.
        /// </summary>
        public List<MediaContent> Search(string keyword)
        {
            return _searchEngine.SearchByKeyword(keyword);
        }

        /// <summary>
        /// Obține statistici complete.
        /// </summary>
        public string GetPlatformStats()
        {
            return $"  Utilizatori: {_users.Count}\n" +
                   $"  Notificări trimise: {_notificationService.GetCount()}\n" +
                   $"  {PlatformManager.Instance.GetStatus()}";
        }
    }
}