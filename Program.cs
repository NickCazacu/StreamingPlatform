using System;
using System.Collections.Generic;
using StreamingPlatform.Models;
using StreamingPlatform.Repositories;
using StreamingPlatform.Services;
using StreamingPlatform.Interfaces;
using StreamingPlatform.Factories;
using StreamingPlatform.Factories.UI;
using StreamingPlatform.Builders;
using StreamingPlatform.Adapters;
using StreamingPlatform.Composite;
using StreamingPlatform.Facade;

namespace StreamingPlatform
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     PLATFORMĂ DE STREAMING                                  ║");
            Console.WriteLine("║     SOLID + Creational Patterns + Structural Patterns        ║");
            Console.WriteLine("║     Student: Nichita | Grupa: TI-233 | UTM                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // ============================================================
            // DEMO 1: CODUL EXISTENT (SOLID)
            // ============================================================

            var userRepo = new InMemoryRepository<User>();
            var contentRepo = new InMemoryRepository<MediaContent>();

            var userService = new UserService(userRepo, contentRepo);
            var contentService = new ContentService(contentRepo);
            var platform = new Services.StreamingPlatform(userService, contentService);

            Console.WriteLine("1. CREARE UTILIZATORI");
            Console.WriteLine("----------------------------------------");

            var user1 = new User("John Doe", "john@email.com", 25);
            user1.Subscription.Upgrade(SubscriptionType.Premium, 6);
            userService.AddUser(user1);

            var user2 = new User("Jane Smith", "jane@email.com", 15);
            userService.AddUser(user2);

            Console.WriteLine(user1.GetInfo());
            Console.WriteLine();
            Console.WriteLine(user2.GetInfo());
            Console.WriteLine();

            Console.WriteLine("2. CREARE CONȚINUT");
            Console.WriteLine("----------------------------------------");

            var movie1 = new Movie(
                "Reservoir Dogs",
                "You've been brave enough. For one day",
                Genre.Action,
                ContentRating.R,
                99,
                "Quentin Tarantino"
            );
            movie1.AddCastMember("Tim Roth");
            movie1.AddCastMember("Michael Madsen");
            contentService.AddContent(movie1);

            var movie2 = new Movie(
                "The Dark Knight",
                "Batman faces the Joker",
                Genre.Action,
                ContentRating.PG13,
                152,
                "Christopher Nolan"
            );
            contentService.AddContent(movie2);

            var series1 = new Series(
                "House M.D",
                "Everybody Lies",
                Genre.Drama,
                ContentRating.R,
                "David Shore",
                8,
                177,
                50
            );
            contentService.AddContent(series1);

            Console.WriteLine(movie1.GetInfo());
            Console.WriteLine();
            Console.WriteLine(series1.GetInfo());
            Console.WriteLine();

            Console.WriteLine("3. ADĂUGARE RATING-URI");
            Console.WriteLine("----------------------------------------");

            platform.RateContent(movie1.Id, 5);
            platform.RateContent(movie1.Id, 4.5);
            platform.RateContent(movie1.Id, 5);

            platform.RateContent(series1.Id, 4.8);
            platform.RateContent(series1.Id, 5);

            Console.WriteLine($"Reservoir Dogs evaluat: {movie1.AverageRating}/5");
            Console.WriteLine($"House M.D evaluat: {series1.AverageRating}/5");
            Console.WriteLine();

            Console.WriteLine("4. VIZIONARE CONȚINUT");
            Console.WriteLine("----------------------------------------");

            try
            {
                platform.PlayContent(user1.Id, movie1.Id);
                platform.AddToFavorites(user1.Id, movie1.Id);

                platform.PlayContent(user1.Id, series1.Id);

                platform.PlayContent(user2.Id, movie2.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare: {ex.Message}");
            }

            Console.WriteLine();

            Console.WriteLine("5. INFORMAȚII UTILIZATORI ACTUALIZATE");
            Console.WriteLine("----------------------------------------");

            Console.WriteLine(user1.GetInfo());
            Console.WriteLine();
            Console.WriteLine(user2.GetInfo());
            Console.WriteLine();

            Console.WriteLine("6. INFORMAȚII CONȚINUT ACTUALIZAT");
            Console.WriteLine("----------------------------------------");

            Console.WriteLine(movie1.GetInfo());
            Console.WriteLine();
            Console.WriteLine(series1.GetInfo());
            Console.WriteLine();

            Console.WriteLine("========================================");
            Console.WriteLine("DEMONSTRAȚIE SOLID FINALIZATĂ");
            Console.WriteLine("========================================\n");

            // ============================================================
            // DEMO 2-6: CREATIONAL PATTERNS (din laboratoarele anterioare)
            // ============================================================
            DemoFactoryMethod();
            DemoAbstractFactory();
            DemoBuilder();
            DemoPrototype();
            DemoSingleton();

            // ============================================================
            // DEMO 7-9: STRUCTURAL PATTERNS (laborator 4)
            // ============================================================
            DemoAdapter();
            DemoComposite();
            DemoFacade();

            Console.WriteLine("\n════════════════════════════════════════════════════════════");
            Console.WriteLine("  TOATE DEMONSTRAȚIILE AU FOST FINALIZATE CU SUCCES!");
            Console.WriteLine("════════════════════════════════════════════════════════════");
        }

        // ============================================================
        // DEMO 2: FACTORY METHOD
        // ============================================================
        static void DemoFactoryMethod()
        {
            Console.WriteLine("┌──────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  DEMO 2: FACTORY METHOD PATTERN                          │");
            Console.WriteLine("└──────────────────────────────────────────────────────────┘\n");

            var movieFactory = new MovieCreator("Steven Spielberg", 150);
            movieFactory.CreateAndRegister("Jurassic Park", "Dinosaurs return to life",
                Genre.SciFi, ContentRating.PG13, 127, "Steven Spielberg");
            movieFactory.CreateAndRegister("Schindler's List", "A story of courage",
                Genre.Drama, ContentRating.R, 195, "Steven Spielberg");

            var seriesFactory = new SeriesCreator("Unknown", 3, 10, 55);
            seriesFactory.CreateAndRegister("Stranger Things", "Supernatural kids",
                Genre.SciFi, ContentRating.PG13, "The Duffer Brothers", 4, 34, 55);

            var docFactory = new DocumentaryCreator("David Attenborough", 60);
            docFactory.CreateAndRegister("Planet Earth", "Natural world",
                Genre.Documentary, ContentRating.G, 60, "Nature", "David Attenborough");

            Console.WriteLine($"\n  Total: Movie={movieFactory.GetCreatedCount()}, Series={seriesFactory.GetCreatedCount()}, Doc={docFactory.GetCreatedCount()}");
            Console.WriteLine("  [Factory Method] Clientul nu cunoaște clasa concretă\n");
        }

        // ============================================================
        // DEMO 3: ABSTRACT FACTORY
        // ============================================================
        static void DemoAbstractFactory()
        {
            Console.WriteLine("┌──────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  DEMO 3: ABSTRACT FACTORY PATTERN                        │");
            Console.WriteLine("└──────────────────────────────────────────────────────────┘\n");

            PlatformTheme[] platforms = { PlatformTheme.Windows, PlatformTheme.Mac, PlatformTheme.Linux };
            foreach (var p in platforms)
            {
                var app = new StreamingApp(UIFactoryProvider.GetFactory(p));
                Console.WriteLine($"  [{p}] {app.PlayContent("Reservoir Dogs")}");
            }
            Console.WriteLine("\n  [Abstract Factory] Familii de obiecte consistente per platformă\n");
        }

        // ============================================================
        // DEMO 4: BUILDER PATTERN
        // ============================================================
        static void DemoBuilder()
        {
            Console.WriteLine("┌──────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  DEMO 4: BUILDER PATTERN                                 │");
            Console.WriteLine("└──────────────────────────────────────────────────────────┘\n");

            var movie = new MovieBuilder()
                .SetTitle("Pulp Fiction")
                .SetDescription("Interconnected stories of crime in LA")
                .SetGenre(Genre.Action).SetRating(ContentRating.R)
                .SetDuration(154).SetDirector("Quentin Tarantino")
                .AddCastMember("John Travolta").AddCastMember("Samuel L. Jackson")
                .Build();
            Console.WriteLine($"  {movie.Title} | {movie.Director} | {movie.DurationMinutes} min | Cast: {string.Join(", ", movie.Cast)}");

            var director = new ContentDirector();
            var blockbuster = director.BuildBlockbusterMovie(
                new MovieBuilder().SetTitle("Avengers").SetDirector("Russo Brothers"));
            Console.WriteLine($"  Blockbuster: {blockbuster.Title} | {blockbuster.Genre} | {blockbuster.DurationMinutes} min");

            Console.WriteLine("\n  [Builder] Construire pas cu pas + Director cu rețete\n");
        }

        // ============================================================
        // DEMO 5: PROTOTYPE PATTERN
        // ============================================================
        static void DemoPrototype()
        {
            Console.WriteLine("┌──────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  DEMO 5: PROTOTYPE PATTERN                               │");
            Console.WriteLine("└──────────────────────────────────────────────────────────┘\n");

            var original = new Movie("Inception", "Mind-bending thriller",
                Genre.SciFi, ContentRating.PG13, 148, "Christopher Nolan");
            original.AddCastMember("Leonardo DiCaprio");

            var shallow = original.ShallowClone();
            var deep = original.DeepClone();
            deep.Title = "Inception 2";
            deep.AddCastMember("New Actor");

            Console.WriteLine($"  Original: {original.Title} | Cast: {string.Join(", ", original.Cast)}");
            Console.WriteLine($"  Deep Clone: {deep.Title} | Cast: {string.Join(", ", deep.Cast)}");
            Console.WriteLine("  → Deep Clone e independent de original");
            Console.WriteLine("\n  [Prototype] Clonare rapidă + Shallow vs Deep Copy\n");
        }

        // ============================================================
        // DEMO 6: SINGLETON PATTERN
        // ============================================================
        static void DemoSingleton()
        {
            Console.WriteLine("┌──────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  DEMO 6: SINGLETON PATTERN                               │");
            Console.WriteLine("└──────────────────────────────────────────────────────────┘\n");

            var m1 = PlatformManager.Instance;
            var m2 = PlatformManager.Instance;
            Console.WriteLine($"  m1 == m2: {ReferenceEquals(m1, m2)} (aceeași instanță)");
            Console.WriteLine($"  Connection ID: {m1.ConnectionId}");

            PlatformManager.Instance.IncrementUsers();
            PlatformManager.Instance.IncrementStreams();
            Console.WriteLine($"  Users: {PlatformManager.Instance.TotalUsers}, Streams: {PlatformManager.Instance.TotalStreams}");
            Console.WriteLine("\n  [Singleton] O singură instanță globală + thread-safe\n");
        }

        // ============================================================
        // DEMO 7: ADAPTER PATTERN
        // ============================================================
        static void DemoAdapter()
        {
            Console.WriteLine("┌──────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  DEMO 7: ADAPTER PATTERN                                 │");
            Console.WriteLine("└──────────────────────────────────────────────────────────┘\n");

            // Creăm serviciile externe (API-uri incompatibile)
            var imdb = new ImdbService();          // Scală 1-10, metode: FetchImdbScore()
            var rt = new RottenTomatoesService();   // Scală 0-100%, metode: GetTomatoMeter()
            var mc = new MetacriticService();       // Scală 0-100, metode: RetrieveMetascore()

            // Creăm adaptoarele — transformă API-urile diferite în IExternalRatingService
            IExternalRatingService imdbAdapter = new ImdbAdapter(imdb);
            IExternalRatingService rtAdapter = new RottenTomatoesAdapter(rt);
            IExternalRatingService mcAdapter = new MetacriticAdapter(mc);

            // Demonstrăm că toate sunt acum UNIFORME
            Console.WriteLine("  --- Rating-uri individuale (interfață uniformă) ---\n");

            string[] titles = { "Reservoir Dogs", "The Dark Knight", "Inception", "Breaking Bad" };

            foreach (var title in titles)
            {
                Console.WriteLine($"  '{title}':");
                Console.WriteLine($"    {imdbAdapter.GetServiceName()}: {imdbAdapter.GetRating(title)}/10");
                Console.WriteLine($"    {rtAdapter.GetServiceName()}: {rtAdapter.GetRating(title)}/10");
                Console.WriteLine($"    {mcAdapter.GetServiceName()}: {mcAdapter.GetRating(title)}/10");
                Console.WriteLine();
            }

            // Demonstrăm RatingAggregator — lucrează cu orice IExternalRatingService
            Console.WriteLine("  --- RatingAggregator (tratare uniformă) ---\n");

            var aggregator = new RatingAggregator();
            aggregator.AddService(imdbAdapter);
            aggregator.AddService(rtAdapter);
            aggregator.AddService(mcAdapter);

            foreach (var title in titles)
            {
                Console.WriteLine($"  '{title}' — Media: {aggregator.GetAverageRating(title)}/10");
            }

            Console.WriteLine();

            // Review-uri de la toate serviciile
            Console.WriteLine("  --- Review-uri agregate pentru 'The Dark Knight' ---\n");
            Console.WriteLine(aggregator.GetAllReviews("The Dark Knight"));

            Console.WriteLine("  [Adapter] 3 API-uri diferite → 1 interfață uniformă (IExternalRatingService)");
            Console.WriteLine("  Conversii: IMDB 1-10 → 1-10, RT 0-100% → /10, Metacritic 0-100 → /10\n");
        }

        // ============================================================
        // DEMO 8: COMPOSITE PATTERN
        // ============================================================
        static void DemoComposite()
        {
            Console.WriteLine("┌──────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  DEMO 8: COMPOSITE PATTERN                               │");
            Console.WriteLine("└──────────────────────────────────────────────────────────┘\n");

            // Creăm conținut individual (Leaf)
            var inception = new MediaLeaf(new Movie("Inception", "Mind-bending thriller",
                Genre.SciFi, ContentRating.PG13, 148, "Christopher Nolan"));
            var darkKnight = new MediaLeaf(new Movie("The Dark Knight", "Batman vs Joker",
                Genre.Action, ContentRating.PG13, 152, "Christopher Nolan"));
            var interstellar = new MediaLeaf(new Movie("Interstellar", "Space exploration",
                Genre.SciFi, ContentRating.PG13, 169, "Christopher Nolan"));

            var breakingBad = new MediaLeaf(new Series("Breaking Bad", "Chemistry teacher turns criminal",
                Genre.Drama, ContentRating.R, "Vince Gilligan", 5, 62, 47));
            var strangerThings = new MediaLeaf(new Series("Stranger Things", "Supernatural kids",
                Genre.SciFi, ContentRating.PG13, "Duffer Brothers", 4, 34, 55));

            var planetEarth = new MediaLeaf(new Documentary("Planet Earth", "Nature documentary",
                Genre.Documentary, ContentRating.G, 60, "Nature", "David Attenborough"));
            var cosmos = new MediaLeaf(new Documentary("Cosmos", "The universe explained",
                Genre.Documentary, ContentRating.G, 55, "Space", "Neil deGrasse Tyson"));

            // Creăm playlisturi (Composite)
            Console.WriteLine("  --- Construire ierarhie ---\n");

            // Playlist simplu
            var nolanPlaylist = new MediaPlaylist("Nolan Collection", "Cele mai bune filme Nolan");
            nolanPlaylist.Add(inception);
            nolanPlaylist.Add(darkKnight);
            nolanPlaylist.Add(interstellar);

            // Alt playlist simplu
            var serialePlaylist = new MediaPlaylist("Seriale Top", "Seriale de vizionat");
            serialePlaylist.Add(breakingBad);
            serialePlaylist.Add(strangerThings);

            // Playlist de documentare
            var docPlaylist = new MediaPlaylist("Documentare", "Documentare educaționale");
            docPlaylist.Add(planetEarth);
            docPlaylist.Add(cosmos);

            // MEGA PLAYLIST — conține alte playlisturi (ierarhie pe niveluri)
            var megaPlaylist = new MediaPlaylist("★ Weekend Binge ★", "Tot ce trebuie pentru weekend");
            megaPlaylist.Add(nolanPlaylist);      // playlist cu 3 filme
            megaPlaylist.Add(serialePlaylist);    // playlist cu 2 seriale
            megaPlaylist.Add(docPlaylist);        // playlist cu 2 documentare

            // Afișăm ierarhia completă
            Console.WriteLine(megaPlaylist.Display("  "));
            Console.WriteLine();

            // Tratare UNIFORMĂ — aceleași metode pe Leaf și Composite
            Console.WriteLine("  --- Tratare uniformă (Leaf vs Composite) ---\n");

            IMediaComponent[] components = { inception, nolanPlaylist, megaPlaylist };
            foreach (var component in components)
            {
                Console.WriteLine($"  {component.GetName()}:");
                Console.WriteLine($"    Elemente: {component.GetItemCount()}");
                Console.WriteLine($"    Durată totală: {component.GetTotalDuration()} min ({component.GetTotalDuration() / 60}h {component.GetTotalDuration() % 60}m)");
                Console.WriteLine();
            }

            // Adăugare/eliminare dinamică
            Console.WriteLine("  --- Modificare dinamică ---\n");
            Console.WriteLine($"  Mega Playlist înainte: {megaPlaylist.GetItemCount()} elemente");

            var bonusMovie = new MediaLeaf(new Movie("Tenet", "Time inversion",
                Genre.SciFi, ContentRating.PG13, 150, "Christopher Nolan"));
            nolanPlaylist.Add(bonusMovie);

            Console.WriteLine($"  Mega Playlist după adăugare Tenet: {megaPlaylist.GetItemCount()} elemente");
            Console.WriteLine("  → Adăugarea în sub-playlist se reflectă automat în mega-playlist!");

            Console.WriteLine("\n  [Composite] Film individual și playlist cu 100 filme — aceleași metode");
            Console.WriteLine("  GetTotalDuration(), GetItemCount() funcționează recursiv pe toată ierarhia\n");
        }

        // ============================================================
        // DEMO 9: FAÇADE PATTERN
        // ============================================================
        static void DemoFacade()
        {
            Console.WriteLine("┌──────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  DEMO 9: FAÇADE PATTERN                                  │");
            Console.WriteLine("└──────────────────────────────────────────────────────────┘\n");

            // Creăm fațada — O SINGURĂ clasă care ascunde complexitatea
            var facade = new StreamingFacade();

            // 1. Înregistrare utilizatori (1 metodă = creare + abonament + notificare + log)
            Console.WriteLine("  --- Înregistrare utilizatori ---\n");
            facade.RegisterUser("Alex", "alex@email.com", 25, SubscriptionType.Premium, 12);
            facade.RegisterUser("Maria", "maria@email.com", 14, SubscriptionType.Standard, 3);
            facade.RegisterUser("Ion", "ion@email.com", 30, SubscriptionType.Free);
            Console.WriteLine();

            // 2. Adaugă conținut
            Console.WriteLine("  --- Adăugare conținut ---\n");
            facade.AddContent(new Movie("Reservoir Dogs", "Tarantino's debut",
                Genre.Action, ContentRating.R, 99, "Quentin Tarantino"));
            facade.AddContent(new Movie("The Dark Knight", "Batman vs Joker",
                Genre.Action, ContentRating.PG13, 152, "Christopher Nolan"));
            facade.AddContent(new Movie("Inception", "Mind-bending thriller",
                Genre.SciFi, ContentRating.PG13, 148, "Christopher Nolan"));
            facade.AddContent(new Documentary("Planet Earth", "Nature documentary",
                Genre.Documentary, ContentRating.G, 60, "Nature", "David Attenborough"));
            Console.WriteLine();

            // 3. Vizionare conținut — 1 metodă face 8 pași intern
            Console.WriteLine("  --- Vizionare conținut (1 metodă = 8 pași interni) ---");

            // Alex (Premium, 25 ani) → poate vedea tot
            Console.WriteLine(facade.WatchContent("Alex", "Reservoir Dogs"));
            Console.WriteLine(facade.WatchContent("Alex", "The Dark Knight"));

            // Maria (Standard, 14 ani) → nu poate vedea R-rated
            Console.WriteLine(facade.WatchContent("Maria", "Reservoir Dogs"));

            // Maria poate vedea PG13
            Console.WriteLine(facade.WatchContent("Maria", "The Dark Knight"));

            // Ion (Free) → doar PG și G
            Console.WriteLine(facade.WatchContent("Ion", "The Dark Knight"));
            Console.WriteLine(facade.WatchContent("Ion", "Planet Earth"));

            // Conținut inexistent
            Console.WriteLine(facade.WatchContent("Alex", "Film Inexistent"));
            Console.WriteLine();

            // 4. Info completă (internă + rating-uri externe prin Adapter)
            Console.WriteLine("  --- Informații complete (intern + extern prin Adapter) ---\n");
            Console.WriteLine(facade.GetContentInfo("Reservoir Dogs"));

            // 5. Statistici platformă
            Console.WriteLine("  --- Statistici platformă ---\n");
            Console.WriteLine(facade.GetPlatformStats());

            Console.WriteLine("\n  [Façade] Clientul scrie facade.WatchContent('Alex', 'Inception')");
            Console.WriteLine("  Intern se verifică: user, abonament, vârstă, căutare, rating extern, redare, log, notificare");
            Console.WriteLine("  Complexitatea e ASCUNSĂ — clientul nu știe de subsisteme\n");
        }
    }
}