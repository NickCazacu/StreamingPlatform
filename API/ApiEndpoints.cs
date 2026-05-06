using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StreamingPlatform.Models;
using StreamingPlatform.Models.Db;
using StreamZoneDbContext = StreamingPlatform.Auth.StreamZoneDbContext;
using StreamingPlatform.Builders;
using StreamingPlatform.Adapters;
using StreamingPlatform.Services;
using StreamingPlatform.Proxy;
// IEmailService e în StreamingPlatform.Services — deja inclus
using StreamingPlatform.Flyweight;
using StreamingPlatform.Decorator;
using StreamingPlatform.Bridge;
using StreamingPlatform.Behavioral.Strategy;
using StreamingPlatform.Behavioral.Observer;
using StreamingPlatform.Behavioral.Command;
using StreamingPlatform.Behavioral.Memento;
using StreamingPlatform.Behavioral.Iterator;

namespace StreamingPlatform.Api
{
    /// <summary>
    /// API Endpoints — conectează backend-ul C# cu frontend-ul HTML.
    /// Folosește ASP.NET Minimal API pentru a expune datele ca JSON.
    /// </summary>
    public static class ApiEndpoints
    {
        // Stocăm datele în memorie (în producție ar fi baza de date)
        private static List<Movie> _movies = new List<Movie>();
        private static List<Series> _series = new List<Series>();
        private static List<Documentary> _documentaries = new List<Documentary>();
        private static RatingAggregator _ratingAggregator = new RatingAggregator();

        // ── Registrul utilizatorilor (Proxy Pattern) ─────────────────────
        public static Dictionary<string, UserProfile> UserRegistry { get; } = new()
        {
            ["Ion Cojocaru"]  = new UserProfile("Ion Cojocaru",  25, SubscriptionType.Premium),
            ["Maria Rusu"]    = new UserProfile("Maria Rusu",    15, SubscriptionType.Standard),
            ["Andrei Lupan"]  = new UserProfile("Andrei Lupan",  30, SubscriptionType.Free),
            ["Elena Moraru"]  = new UserProfile("Elena Moraru",  19, SubscriptionType.Standard),
        };

        // ── Sesiuni active Flyweight ──────────────────────────────────────
        private static List<StreamingSession> _activeSessions = new();

        // ── Lab 6 — state ────────────────────────────────────────────────
        private static readonly ContentPublisher _contentPublisher = new();
        private static readonly Dictionary<string, Watchlist> _watchlists = new();
        private static readonly Dictionary<string, CommandHistory> _commandHistories = new();
        private static readonly Dictionary<string, (UserSessionState State, SessionHistory History)> _sessions = new();

        public static List<MediaContent> GetAllContent()
        {
            var all = new List<MediaContent>();
            all.AddRange(_movies);
            all.AddRange(_series);
            all.AddRange(_documentaries);
            return all;
        }

        /// <summary>
        /// Încarcă datele inițiale: dacă BD are deja conținut, citește de acolo;
        /// dacă BD e goală, rulează seed-ul hardcoded (Builder/Factory) și salvează în BD.
        /// Dacă db == null, funcționează doar în memorie (fallback).
        /// </summary>
        public static void LoadData(StreamZoneDbContext? db = null)
        {
            // ── Path 1: BD disponibilă și are deja date → încarcă de acolo ──
            if (db != null)
            {
                try
                {
                    if (db.Movies.Any() || db.SeriesItems.Any() || db.Documentaries.Any())
                    {
                        LoadFromDatabase(db);
                        PlatformManager.Instance.Log("API: Date încărcate din StreamZoneDB.");
                        Console.WriteLine($"  ✅ Date încărcate din BD: {_movies.Count} filme, {_series.Count} seriale, {_documentaries.Count} documentare.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ⚠ Eroare la citire BD: {ex.Message}. Folosesc date hardcoded.");
                }
            }

            // ── Path 2: BD goală sau indisponibilă → rulează seed hardcoded ─
            LoadFromHardcoded();

            // ── Path 3: dacă BD e disponibilă, salvează seed-ul în BD ───────
            if (db != null)
            {
                try
                {
                    SaveToDatabase(db);
                    Console.WriteLine($"  ✅ Seed salvat în BD: {_movies.Count} filme, {_series.Count} seriale, {_documentaries.Count} documentare.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ⚠ Seed în BD a eșuat: {ex.Message}");
                }
            }
        }

        /// <summary>Citește din BD în listele in-memory (_movies/_series/_documentaries).</summary>
        private static void LoadFromDatabase(StreamZoneDbContext db)
        {
            _movies.Clear();
            _series.Clear();
            _documentaries.Clear();

            foreach (var m in db.Movies.AsNoTracking().ToList())
            {
                var movie = new Movie(m.Title, m.Description,
                    ParseGenre(m.Genre), ParseRating(m.ContentRating),
                    m.DurationMinutes, m.Director);
                if (m.AverageRating >= 1) movie.AddRating((double)m.AverageRating);
                _movies.Add(movie);
            }

            foreach (var s in db.SeriesItems.AsNoTracking().ToList())
            {
                var series = new Series(s.Title, s.Description,
                    ParseGenre(s.Genre), ParseRating(s.ContentRating),
                    s.Creator, s.SeasonsCount, s.EpisodesCount, s.EpisodeDuration);
                series.IsCompleted = s.IsCompleted;
                if (s.AverageRating >= 1) series.AddRating((double)s.AverageRating);
                _series.Add(series);
            }

            foreach (var d in db.Documentaries.AsNoTracking().ToList())
            {
                var doc = new Documentary(d.Title, d.Description,
                    ParseGenre(d.Genre), ParseRating(d.ContentRating),
                    d.DurationMinutes, d.Topic, d.Narrator);
                doc.IsEducational = d.IsEducational;
                if (d.AverageRating >= 1) doc.AddRating((double)d.AverageRating);
                _documentaries.Add(doc);
            }

            // Rating Aggregator (Adapter Pattern) — la fel ca în varianta hardcoded
            _ratingAggregator.AddService(new ImdbAdapter(new ImdbService()));
            _ratingAggregator.AddService(new RottenTomatoesAdapter(new RottenTomatoesService()));
            _ratingAggregator.AddService(new MetacriticAdapter(new MetacriticService()));
        }

        /// <summary>Salvează listele in-memory în BD (folosit la primul rul-up).</summary>
        private static void SaveToDatabase(StreamZoneDbContext db)
        {
            // Lista titlurilor MOLDOVENEȘTI (hardcoded — corespunde cu LoadFromHardcoded).
            var moldovanMovies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Carbon", "Hotarul", "Abis", "Puterea Probabilitatii", "Lăutarii",
                "Maria Mirabela", "Nunta în Basarabia", "Tatăl meu, dictatorul",
                "Ce lume minunată", "La limita de jos a cerului", "Dimitrie Cantemir", "Codru"
            };
            var moldovanSeries = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Plaha", "Cumpenele Familiei"
            };
            var moldovanDocs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Moldova: Inima de Vin", "Chișinău — Memorii", "Codrii Moldovei",
                "Ștefan cel Mare — Legenda Moldovei", "Mănăstirile Basarabiei"
            };

            foreach (var m in _movies)
            {
                db.Movies.Add(new MovieEntity
                {
                    Title = m.Title,
                    Description = m.Description,
                    Genre = m.Genre.ToString(),
                    ContentRating = m.Rating.ToString(),
                    DurationMinutes = m.DurationMinutes,
                    Director = m.Director,
                    AverageRating = (decimal)m.AverageRating,
                    IsMoldovan = moldovanMovies.Contains(m.Title)
                });
            }
            foreach (var s in _series)
            {
                db.SeriesItems.Add(new SeriesEntity
                {
                    Title = s.Title,
                    Description = s.Description,
                    Genre = s.Genre.ToString(),
                    ContentRating = s.Rating.ToString(),
                    Creator = s.Creator,
                    SeasonsCount = s.SeasonsCount,
                    EpisodesCount = s.EpisodesCount,
                    EpisodeDuration = s.AverageEpisodeDuration,
                    IsCompleted = s.IsCompleted,
                    AverageRating = (decimal)s.AverageRating,
                    IsMoldovan = moldovanSeries.Contains(s.Title)
                });
            }
            foreach (var d in _documentaries)
            {
                db.Documentaries.Add(new DocumentaryEntity
                {
                    Title = d.Title,
                    Description = d.Description,
                    Genre = d.Genre.ToString(),
                    ContentRating = d.Rating.ToString(),
                    DurationMinutes = d.DurationMinutes,
                    Topic = d.Topic,
                    Narrator = d.Narrator,
                    IsEducational = d.IsEducational,
                    AverageRating = (decimal)d.AverageRating,
                    IsMoldovan = moldovanDocs.Contains(d.Title)
                });
            }
            db.SaveChanges();
        }

        private static Genre ParseGenre(string s) =>
            Enum.TryParse<Genre>(s, true, out var g) ? g : Genre.Drama;

        private static ContentRating ParseRating(string s) =>
            Enum.TryParse<ContentRating>(s, true, out var r) ? r : ContentRating.PG;

        /// <summary>Codul existent — păstrează demo-ul Builder/Factory pentru profesor.</summary>
        public static void LoadFromHardcoded()
        {
            // ── Filme moldovenești ──────────────────────────────────────────────
            var movie1 = new Movie("Carbon", "Un tânăr inginer descoperă că fabrica unde lucrează ascunde secrete toxice periculoase. O dramă contemporană din nordul Moldovei.",
                Genre.Drama, ContentRating.PG13, 102, "Ion Borș");
            movie1.AddCastMember("Valeriu Andriuță");
            movie1.AddCastMember("Irina Iosub");
            movie1.AddCastMember("Mihai Ciobanu");

            var movie2 = new Movie("Hotarul", "Povestea unui sat moldovenesc aflat la frontiera dintre două lumi, în primii ani postbelici. Un film clasic al cinematografiei naționale.",
                Genre.Drama, ContentRating.PG, 88, "Vasile Pascaru");
            movie2.AddCastMember("Dumitru Caraciobanu");
            movie2.AddCastMember("Svetlana Toma");

            var movie3 = new MovieBuilder()
                .SetTitle("Abis")
                .SetDescription("Un actor din Chișinău se luptă cu demonii interiori pe scena vieții și a teatrului. O poveste despre cădere și redempțiune în Moldova contemporană.")
                .SetGenre(Genre.Drama).SetRating(ContentRating.R)
                .SetDuration(96).SetDirector("Vlad Druc")
                .AddCastMembers("Anatol Durbală", "Tatiana Căldare", "Ion Sapdaru")
                .Build();

            var movie4 = new MovieBuilder()
                .SetTitle("Puterea Probabilitatii")
                .SetDescription("Drumurile a trei personaje din Moldova se încrucișează într-un joc al destinului și al alegerilor. O reflecție despre șansă, vinovăție și iertare.")
                .SetGenre(Genre.Drama).SetRating(ContentRating.PG13)
                .SetDuration(110).SetDirector("Anatol Durbală")
                .AddCastMembers("Mihai Fusu", "Olga Grigorescu", "Victor Ciutac")
                .Build();

            var movie5 = new MovieBuilder()
                .SetTitle("Lăutarii")
                .SetDescription("Povestea lui Toma Alimoș, lăutar din Basarabia, între dragoste, libertate și muzica sufletului. Capodopera lui Emil Loteanu, premiată la Moscova.")
                .SetGenre(Genre.Drama).SetRating(ContentRating.PG)
                .SetDuration(87).SetDirector("Emil Loteanu")
                .AddCastMembers("Lăutar Gică Petrescu", "Svetlana Toma", "Grigore Grigoriu")
                .Build();

            // ── Filme moldovenești adiționale ───────────────────────────────────
            var movie6 = new MovieBuilder()
                .SetTitle("Maria Mirabela")
                .SetDescription("Capodopera de animație româno-sovietică a lui Ion Popescu-Gopo. O fetiță pornește într-o călătorie magică prin pădure pentru a-și salva prietenii. Copilărie pură pentru toate generațiile.")
                .SetGenre(Genre.Fantasy).SetRating(ContentRating.G)
                .SetDuration(64).SetDirector("Ion Popescu-Gopo")
                .AddCastMembers("Medeea Marinescu", "Ingrid Vlasov", "Gică Petrescu")
                .Build();

            var movie7 = new MovieBuilder()
                .SetTitle("Nunta în Basarabia")
                .SetDescription("Un cuplu mixt — el bucureștean, ea din Chișinău — descoperă că o nuntă în Basarabia poate uni sau dezbina două lumi. Comedie romantică despre identitate, familie și tradiții.")
                .SetGenre(Genre.Comedy).SetRating(ContentRating.PG13)
                .SetDuration(95).SetDirector("Napoleon Helmis")
                .AddCastMembers("Vlad Logigan", "Victoria Bobu", "Ion Sapdaru", "Mihai Bisericanu")
                .Build();

            var movie8 = new MovieBuilder()
                .SetTitle("Tatăl meu, dictatorul")
                .SetDescription("Adolescenta Lia se confruntă cu autoritatea tatălui său într-o Moldovă post-sovietică plină de contraste. Dramă intimistă despre libertate, familie și schimbări de epocă.")
                .SetGenre(Genre.Drama).SetRating(ContentRating.PG13)
                .SetDuration(105).SetDirector("Igor Cobileanski")
                .AddCastMembers("Eugenia Nicolas", "Constantin Dogioiu", "Doina Severin")
                .Build();

            var movie9 = new MovieBuilder()
                .SetTitle("Ce lume minunată")
                .SetDescription("Trei generații dintr-o familie din Chișinău se reîntâlnesc la o petrecere care scoate la iveală secretele vechi. O comedie tristă despre viața în Moldova de astăzi.")
                .SetGenre(Genre.Drama).SetRating(ContentRating.PG13)
                .SetDuration(98).SetDirector("Anatol Durbală")
                .AddCastMembers("Mihai Fusu", "Olga Grigorescu", "Petru Hadârcă")
                .Build();

            var movie10 = new MovieBuilder()
                .SetTitle("La limita de jos a cerului")
                .SetDescription("Doi frați dintr-un sat din nordul Moldovei visează să evadeze din rutina existenței rurale. O metaforă vizuală despre dor, iertare și destin.")
                .SetGenre(Genre.Drama).SetRating(ContentRating.PG13)
                .SetDuration(92).SetDirector("Igor Cobileanski")
                .AddCastMembers("Igor Babiac", "Dorian Boguță", "Sergiu Voloc")
                .Build();

            var movie11 = new MovieBuilder()
                .SetTitle("Dimitrie Cantemir")
                .SetDescription("Portretul savantului-domnitor moldovean Dimitrie Cantemir, învățat enciclopedic și prinț al Moldovei. Frescă istorică cu accent pe dilemele între politică, cultură și putere.")
                .SetGenre(Genre.Drama).SetRating(ContentRating.PG)
                .SetDuration(118).SetDirector("Vlad Iovita")
                .AddCastMembers("Mihai Volontir", "Ion Ungureanu", "Dumitru Caraciobanu")
                .Build();

            var movie12 = new MovieBuilder()
                .SetTitle("Codru")
                .SetDescription("Un pădurar din codrii Orheiului descoperă o rețea de tăieri ilegale. Thriller ecologic care îmbină frumusețea peisajului basarabean cu mizele morale ale unui om singur.")
                .SetGenre(Genre.Drama).SetRating(ContentRating.PG13)
                .SetDuration(101).SetDirector("Eugen Damaschin")
                .AddCastMembers("Valeriu Andriuță", "Ion Sapdaru", "Tatiana Lapicus")
                .Build();

            // ── Filme internaționale ────────────────────────────────────────────
            var movie13 = new MovieBuilder()
                .SetTitle("Inception")
                .SetDescription("Un hoț specializat în furtul informațiilor din vise primește o ultimă misiune: implantarea unei idei în mintea cuiva. Thriller SF labirintic semnat de Christopher Nolan.")
                .SetGenre(Genre.SciFi).SetRating(ContentRating.PG13)
                .SetDuration(148).SetDirector("Christopher Nolan")
                .AddCastMembers("Leonardo DiCaprio", "Joseph Gordon-Levitt", "Marion Cotillard", "Tom Hardy")
                .Build();

            var movie14 = new MovieBuilder()
                .SetTitle("The Dark Knight")
                .SetDescription("Batman se confruntă cu Joker, un criminal care vrea să arunce Gotham-ul în haos. Magnum opus al genului super-erou, cu o interpretare iconică a lui Heath Ledger.")
                .SetGenre(Genre.Action).SetRating(ContentRating.PG13)
                .SetDuration(152).SetDirector("Christopher Nolan")
                .AddCastMembers("Christian Bale", "Heath Ledger", "Aaron Eckhart", "Gary Oldman")
                .Build();

            var movie15 = new MovieBuilder()
                .SetTitle("Interstellar")
                .SetDescription("Pământul moare. Un grup de astronauți trece printr-o gaură de vierme în căutarea unei planete locuibile pentru omenire. Epopeea SF a lui Nolan despre timp, dragoste și supraviețuire.")
                .SetGenre(Genre.SciFi).SetRating(ContentRating.PG13)
                .SetDuration(169).SetDirector("Christopher Nolan")
                .AddCastMembers("Matthew McConaughey", "Anne Hathaway", "Jessica Chastain", "Michael Caine")
                .Build();

            var movie16 = new MovieBuilder()
                .SetTitle("The Shawshank Redemption")
                .SetDescription("Andy Dufresne, condamnat pe nedrept la închisoare pe viață, își păstrează speranța alături de prietenul său Red. Cea mai votată dramă a tuturor timpurilor.")
                .SetGenre(Genre.Drama).SetRating(ContentRating.R)
                .SetDuration(142).SetDirector("Frank Darabont")
                .AddCastMembers("Tim Robbins", "Morgan Freeman", "Bob Gunton")
                .Build();

            _movies = new List<Movie>
            {
                movie1, movie2, movie3, movie4, movie5,
                movie6, movie7, movie8, movie9, movie10,
                movie11, movie12, movie13, movie14, movie15, movie16
            };

            // Adăugăm rating-uri
            movie1.AddRating(4.2); movie1.AddRating(4.5); movie1.AddRating(4.0);
            movie2.AddRating(4.6); movie2.AddRating(5.0); movie2.AddRating(4.8);
            movie3.AddRating(3.9); movie3.AddRating(4.1); movie3.AddRating(4.3);
            movie4.AddRating(4.0); movie4.AddRating(4.2); movie4.AddRating(4.5);
            movie5.AddRating(4.8); movie5.AddRating(5.0); movie5.AddRating(4.9);
            movie6.AddRating(4.7); movie6.AddRating(4.9); movie6.AddRating(4.6);
            movie7.AddRating(4.1); movie7.AddRating(3.9); movie7.AddRating(4.2);
            movie8.AddRating(4.3); movie8.AddRating(4.0); movie8.AddRating(4.4);
            movie9.AddRating(3.8); movie9.AddRating(4.1); movie9.AddRating(3.9);
            movie10.AddRating(4.2); movie10.AddRating(4.4); movie10.AddRating(4.0);
            movie11.AddRating(4.5); movie11.AddRating(4.3); movie11.AddRating(4.6);
            movie12.AddRating(3.7); movie12.AddRating(4.0); movie12.AddRating(3.9);
            movie13.AddRating(4.7); movie13.AddRating(4.8); movie13.AddRating(4.9);
            movie14.AddRating(4.9); movie14.AddRating(5.0); movie14.AddRating(4.8);
            movie15.AddRating(4.6); movie15.AddRating(4.7); movie15.AddRating(4.5);
            movie16.AddRating(5.0); movie16.AddRating(4.9); movie16.AddRating(5.0);

            // ── Seriale moldovenești ────────────────────────────────────────────
            var series1 = new SeriesBuilder()
                .SetTitle("Plaha")
                .SetDescription("O dramă despre crima organizată, traficul de droguri și oameni prinși între loialitate și supraviețuire. Inspirat din realitățile spațiului post-sovietic.")
                .SetGenre(Genre.Drama).SetRating(ContentRating.R)
                .SetCreator("Iuri Moroz").SetSeasons(1).SetEpisodes(8).SetEpisodeDuration(52)
                .MarkAsCompleted().Build();

            // Serial românesc clasic
            var series2 = new SeriesBuilder()
                .SetTitle("Toate pânzele sus!")
                .SetDescription("Aventurile echipajului bricului Speranța în secolul XIX, după romanul lui Radu Tudoran. Clasic al televiziunii românești despre prietenie, curaj și descoperire.")
                .SetGenre(Genre.Action).SetRating(ContentRating.PG)
                .SetCreator("Mircea Mureșan").SetSeasons(1).SetEpisodes(12).SetEpisodeDuration(48)
                .MarkAsCompleted().Build();

            // Serial moldovenesc (fictiv-realist)
            var series3 = new SeriesBuilder()
                .SetTitle("Cumpenele Familiei")
                .SetDescription("Trei generații dintr-o familie din Bălți încearcă să-și păstreze casa părintească într-o Moldovă care se schimbă rapid. Dramă socială cu accente de comedie.")
                .SetGenre(Genre.Drama).SetRating(ContentRating.PG13)
                .SetCreator("Anatol Durbală").SetSeasons(2).SetEpisodes(20).SetEpisodeDuration(45)
                .MarkAsCompleted().Build();

            // ── Seriale internaționale ──────────────────────────────────────────
            var series4 = new SeriesBuilder()
                .SetTitle("Breaking Bad")
                .SetDescription("Un profesor de chimie diagnosticat cu cancer începe să producă metamfetamină pentru a-și asigura familia. Una dintre cele mai aclamate serii TV din toate timpurile.")
                .SetGenre(Genre.Drama).SetRating(ContentRating.R)
                .SetCreator("Vince Gilligan").SetSeasons(5).SetEpisodes(62).SetEpisodeDuration(49)
                .MarkAsCompleted().Build();

            var series5 = new SeriesBuilder()
                .SetTitle("House M.D.")
                .SetDescription("Dr. Gregory House, geniu medical antisocial, rezolvă cazuri imposibile cu echipa sa de la Princeton-Plainsboro. Mister medical cu suspans și umor caustic.")
                .SetGenre(Genre.Drama).SetRating(ContentRating.PG13)
                .SetCreator("David Shore").SetSeasons(8).SetEpisodes(177).SetEpisodeDuration(44)
                .MarkAsCompleted().Build();

            var series6 = new SeriesBuilder()
                .SetTitle("The Witcher")
                .SetDescription("Geralt din Rivia, vânător mutant de monștri, traversează un continent fantastic în căutarea destinului său. Fantasy epic bazat pe romanele lui Andrzej Sapkowski.")
                .SetGenre(Genre.Fantasy).SetRating(ContentRating.R)
                .SetCreator("Lauren S. Hissrich").SetSeasons(3).SetEpisodes(24).SetEpisodeDuration(60)
                .Build();

            var series7 = new SeriesBuilder()
                .SetTitle("Stranger Things")
                .SetDescription("În anii '80, un grup de copii din Hawkins descoperă un univers paralel și forțe supranaturale. Omagiu nostalgic adus filmelor SF/horror din acea perioadă.")
                .SetGenre(Genre.SciFi).SetRating(ContentRating.PG13)
                .SetCreator("Duffer Brothers").SetSeasons(4).SetEpisodes(34).SetEpisodeDuration(55)
                .Build();

            _series = new List<Series> { series1, series2, series3, series4, series5, series6, series7 };
            series1.AddRating(4.7); series1.AddRating(4.9); series1.AddRating(5.0);
            series2.AddRating(4.8); series2.AddRating(4.9); series2.AddRating(4.7);
            series3.AddRating(4.2); series3.AddRating(4.0); series3.AddRating(4.3);
            series4.AddRating(4.9); series4.AddRating(5.0); series4.AddRating(4.9);
            series5.AddRating(4.7); series5.AddRating(4.8); series5.AddRating(4.6);
            series6.AddRating(4.0); series6.AddRating(4.2); series6.AddRating(3.8);
            series7.AddRating(4.5); series7.AddRating(4.6); series7.AddRating(4.4);

            // ── Documentare moldovenești ────────────────────────────────────────
            var doc1 = new DocumentaryBuilder()
                .SetTitle("Moldova: Inima de Vin")
                .SetDescription("Un portret al viticulturii moldovenești — oameni, podgorii și tradiții care fac din Moldova unul dintre cei mai mari producători de vin din lume.")
                .SetRating(ContentRating.G).SetDuration(52)
                .SetTopic("Cultură").SetNarrator("Nicolae Jelescu")
                .MarkAsEducational().Build();

            var doc2 = new DocumentaryBuilder()
                .SetTitle("Chișinău — Memorii")
                .SetDescription("Mărturii ale locuitorilor bătrâni ai Chișinăului și arhivă vizuală rară care reconturează istoria orașului de-a lungul unui secol de schimbări.")
                .SetRating(ContentRating.G).SetDuration(48)
                .SetTopic("Istorie").SetNarrator("Mihai Cimpoi")
                .MarkAsEducational().Build();

            // ── Documentare adiționale ──────────────────────────────────────────
            var doc3 = new DocumentaryBuilder()
                .SetTitle("Codrii Moldovei")
                .SetDescription("Pădurile centrale ale Moldovei — Codrii — sunt explorate prin obiectivul biologilor și pădurarilor. De la stejarii seculari la fauna sălbatică, un portret al ultimelor zone naturale autentice.")
                .SetRating(ContentRating.G).SetDuration(58)
                .SetTopic("Natură").SetNarrator("Valeriu Munteanu")
                .MarkAsEducational().Build();

            var doc4 = new DocumentaryBuilder()
                .SetTitle("Ștefan cel Mare — Legenda Moldovei")
                .SetDescription("Recompunere documentară a vieții marelui domnitor Ștefan III al Moldovei, învingător în 34 de bătălii și ctitor a peste 40 de mănăstiri. Cu reconstituiri și mărturii ale istoricilor.")
                .SetRating(ContentRating.PG).SetDuration(72)
                .SetTopic("Istorie").SetNarrator("Ion Țurcanu")
                .MarkAsEducational().Build();

            var doc5 = new DocumentaryBuilder()
                .SetTitle("Mănăstirile Basarabiei")
                .SetDescription("Călătorie prin mănăstirile rupestre și de zid ale Moldovei — Țipova, Saharna, Curchi, Căpriana. Spiritualitate, arhitectură și istorie pe malul Nistrului.")
                .SetRating(ContentRating.G).SetDuration(64)
                .SetTopic("Cultură").SetNarrator("Mihai Cimpoi")
                .MarkAsEducational().Build();

            var doc6 = new DocumentaryBuilder()
                .SetTitle("Our Planet")
                .SetDescription("Documentar epic Netflix despre frumusețea și fragilitatea planetei. De la junglele tropicale la deșerturile reci, narat cu vocea inconfundabilă a lui David Attenborough.")
                .SetRating(ContentRating.G).SetDuration(50)
                .SetTopic("Natură").SetNarrator("David Attenborough")
                .MarkAsEducational().Build();

            _documentaries = new List<Documentary> { doc1, doc2, doc3, doc4, doc5, doc6 };
            doc1.AddRating(4.6); doc1.AddRating(4.8); doc1.AddRating(5.0);
            doc2.AddRating(4.4); doc2.AddRating(4.7); doc2.AddRating(4.5);
            doc3.AddRating(4.5); doc3.AddRating(4.6); doc3.AddRating(4.4);
            doc4.AddRating(4.8); doc4.AddRating(4.9); doc4.AddRating(4.7);
            doc5.AddRating(4.3); doc5.AddRating(4.5); doc5.AddRating(4.4);
            doc6.AddRating(4.9); doc6.AddRating(5.0); doc6.AddRating(4.9);

            // Rating Aggregator (Adapter Pattern)
            _ratingAggregator.AddService(new ImdbAdapter(new ImdbService()));
            _ratingAggregator.AddService(new RottenTomatoesAdapter(new RottenTomatoesService()));
            _ratingAggregator.AddService(new MetacriticAdapter(new MetacriticService()));

            // Log în Singleton
            PlatformManager.Instance.Log("API: Date încărcate cu succes");
        }

        /// <summary>
        /// Înregistrează toate endpoint-urile API.
        /// </summary>
        public static void MapEndpoints(WebApplication app)
        {
            // --- FILME ---
            app.MapGet("/api/movies", () =>
            {
                return _movies.Select(m => new
                {
                    id = m.Id,
                    title = m.Title,
                    description = m.Description,
                    genre = m.Genre.ToString(),
                    rating = m.Rating.ToString(),
                    duration = m.DurationMinutes,
                    director = m.Director,
                    cast = m.Cast,
                    views = m.ViewsCount,
                    averageRating = m.AverageRating,
                    type = "Movie"
                });
            });

            app.MapGet("/api/movies/{id}", (int id) =>
            {
                var movie = _movies.FirstOrDefault(m => m.Id == id);
                if (movie == null) return Results.NotFound();
                return Results.Ok(new
                {
                    id = movie.Id,
                    title = movie.Title,
                    description = movie.Description,
                    genre = movie.Genre.ToString(),
                    rating = movie.Rating.ToString(),
                    duration = movie.DurationMinutes,
                    director = movie.Director,
                    cast = movie.Cast,
                    views = movie.ViewsCount,
                    averageRating = movie.AverageRating,
                    externalRating = _ratingAggregator.GetAverageRating(movie.Title),
                    type = "Movie"
                });
            });

            // --- SERIALE ---
            app.MapGet("/api/series", () =>
            {
                return _series.Select(s => new
                {
                    id = s.Id,
                    title = s.Title,
                    description = s.Description,
                    genre = s.Genre.ToString(),
                    rating = s.Rating.ToString(),
                    creator = s.Creator,
                    seasons = s.SeasonsCount,
                    episodes = s.EpisodesCount,
                    episodeDuration = s.AverageEpisodeDuration,
                    totalDuration = s.GetDuration(),
                    isCompleted = s.IsCompleted,
                    views = s.ViewsCount,
                    averageRating = s.AverageRating,
                    type = "Series"
                });
            });

            // --- DOCUMENTARE ---
            app.MapGet("/api/documentaries", () =>
            {
                return _documentaries.Select(d => new
                {
                    id = d.Id,
                    title = d.Title,
                    description = d.Description,
                    genre = d.Genre.ToString(),
                    rating = d.Rating.ToString(),
                    duration = d.DurationMinutes,
                    topic = d.Topic,
                    narrator = d.Narrator,
                    isEducational = d.IsEducational,
                    views = d.ViewsCount,
                    averageRating = d.AverageRating,
                    type = "Documentary"
                });
            });

            // --- TOT CONȚINUTUL ---
            app.MapGet("/api/content", () =>
            {
                var all = new List<object>();

                all.AddRange(_movies.Select(m => new
                {
                    id = m.Id, title = m.Title, description = m.Description,
                    genre = m.Genre.ToString(), contentRating = m.Rating.ToString(),
                    duration = m.DurationMinutes, views = m.ViewsCount,
                    averageRating = m.AverageRating,
                    externalRating = _ratingAggregator.GetAverageRating(m.Title),
                    type = "Movie", director = m.Director,
                    cast = m.Cast
                }));

                all.AddRange(_series.Select(s => new
                {
                    id = s.Id, title = s.Title, description = s.Description,
                    genre = s.Genre.ToString(), contentRating = s.Rating.ToString(),
                    duration = s.GetDuration(), views = s.ViewsCount,
                    averageRating = s.AverageRating,
                    externalRating = _ratingAggregator.GetAverageRating(s.Title),
                    type = "Series", director = s.Creator,
                    cast = new List<string>()
                }));

                all.AddRange(_documentaries.Select(d => new
                {
                    id = d.Id, title = d.Title, description = d.Description,
                    genre = d.Genre.ToString(), contentRating = d.Rating.ToString(),
                    duration = d.DurationMinutes, views = d.ViewsCount,
                    averageRating = d.AverageRating,
                    externalRating = _ratingAggregator.GetAverageRating(d.Title),
                    type = "Documentary", director = d.Narrator,
                    cast = new List<string>()
                }));

                return all.OrderByDescending(x =>
                    ((dynamic)x).averageRating);
            });

            // --- RATING-URI EXTERNE (Adapter Pattern) ---
            app.MapGet("/api/ratings/{title}", (string title) =>
            {
                return new
                {
                    title = title,
                    averageExternal = _ratingAggregator.GetAverageRating(title),
                    details = _ratingAggregator.GetDetailedRatings(title),
                    reviews = _ratingAggregator.GetAllReviews(title)
                };
            });

            // --- STATISTICI (Singleton Pattern) ---
            app.MapGet("/api/stats", () =>
            {
                return new
                {
                    totalMovies = _movies.Count,
                    totalSeries = _series.Count,
                    totalDocumentaries = _documentaries.Count,
                    totalContent = _movies.Count + _series.Count + _documentaries.Count,
                    platformStatus = PlatformManager.Instance.GetStatus(),
                    connectionId = PlatformManager.Instance.ConnectionId,
                    isConnected = PlatformManager.Instance.IsConnected,
                    totalStreams = PlatformManager.Instance.TotalStreams,
                    totalUsers = PlatformManager.Instance.TotalUsers
                };
            });

            // --- CĂUTARE (interogă BD direct — vede orice rând adăugat în SSMS) ---
            app.MapGet("/api/search", async (string? q, string? genre, bool? moldovanOnly, StreamZoneDbContext db) =>
            {
                IQueryable<MovieEntity> movies = db.Movies.AsNoTracking();
                IQueryable<SeriesEntity> series = db.SeriesItems.AsNoTracking();
                IQueryable<DocumentaryEntity> docs = db.Documentaries.AsNoTracking();

                if (!string.IsNullOrEmpty(q))
                {
                    movies = movies.Where(m => m.Title.Contains(q) || m.Description.Contains(q) || m.Director.Contains(q));
                    series = series.Where(s => s.Title.Contains(q) || s.Description.Contains(q) || s.Creator.Contains(q));
                    docs   = docs  .Where(d => d.Title.Contains(q) || d.Description.Contains(q) || d.Narrator.Contains(q));
                }

                if (!string.IsNullOrEmpty(genre))
                {
                    movies = movies.Where(m => m.Genre == genre);
                    series = series.Where(s => s.Genre == genre);
                    docs   = docs  .Where(d => d.Genre == genre);
                }

                if (moldovanOnly == true)
                {
                    movies = movies.Where(m => m.IsMoldovan);
                    series = series.Where(s => s.IsMoldovan);
                    docs   = docs  .Where(d => d.IsMoldovan);
                }

                var movieList  = await movies.ToListAsync();
                var seriesList = await series.ToListAsync();
                var docsList   = await docs.ToListAsync();

                var results = new List<object>();
                results.AddRange(movieList.Select(m => new
                {
                    id = m.MovieId, title = m.Title, description = m.Description,
                    genre = m.Genre, contentRating = m.ContentRating,
                    duration = m.DurationMinutes, director = m.Director,
                    averageRating = m.AverageRating, isMoldovan = m.IsMoldovan,
                    posterUrl = m.PosterUrl, type = "Movie"
                }));
                results.AddRange(seriesList.Select(s => new
                {
                    id = s.SeriesId, title = s.Title, description = s.Description,
                    genre = s.Genre, contentRating = s.ContentRating,
                    duration = s.SeasonsCount * s.EpisodesCount * s.EpisodeDuration,
                    director = s.Creator,
                    averageRating = s.AverageRating, isMoldovan = s.IsMoldovan,
                    posterUrl = s.PosterUrl, type = "Series"
                }));
                results.AddRange(docsList.Select(d => new
                {
                    id = d.DocumentaryId, title = d.Title, description = d.Description,
                    genre = d.Genre, contentRating = d.ContentRating,
                    duration = d.DurationMinutes, director = d.Narrator,
                    averageRating = d.AverageRating, isMoldovan = d.IsMoldovan,
                    posterUrl = d.PosterUrl, type = "Documentary"
                }));

                return Results.Ok(new
                {
                    query = q,
                    genre,
                    moldovanOnly = moldovanOnly == true,
                    totalResults = results.Count,
                    source = "StreamZoneDB",
                    results
                });
            });

            // --- PLAY (incrementează vizualizări) ---
            app.MapPost("/api/play/{title}", (string title) =>
            {
                var content = _movies.FirstOrDefault(m => m.Title.Equals(title, StringComparison.OrdinalIgnoreCase)) as MediaContent
                    ?? _series.FirstOrDefault(s => s.Title.Equals(title, StringComparison.OrdinalIgnoreCase)) as MediaContent
                    ?? _documentaries.FirstOrDefault(d => d.Title.Equals(title, StringComparison.OrdinalIgnoreCase)) as MediaContent;

                if (content == null) return Results.NotFound(new { message = $"'{title}' nu a fost găsit." });

                var result = content.Play();
                PlatformManager.Instance.IncrementStreams();
                PlatformManager.Instance.Log($"API Play: {title}");

                return Results.Ok(new { message = result, views = content.ViewsCount });
            });

            // --- GENURI ---
            app.MapGet("/api/genres", () =>
            {
                return Enum.GetNames<Genre>().Select(g => new
                {
                    name = g,
                    movieCount = _movies.Count(m => m.Genre.ToString() == g),
                    seriesCount = _series.Count(s => s.Genre.ToString() == g),
                    docCount = _documentaries.Count(d => d.Genre.ToString() == g),
                    totalCount = _movies.Count(m => m.Genre.ToString() == g)
                        + _series.Count(s => s.Genre.ToString() == g)
                        + _documentaries.Count(d => d.Genre.ToString() == g)
                });
            });

            // ================================================================
            // PROXY PATTERN — Gestionarea utilizatorilor și controlul accesului
            // ================================================================

            // GET /api/users — listare utilizatori
            app.MapGet("/api/users", () =>
            {
                return UserRegistry.Values.Select(u => new
                {
                    name = u.Name,
                    age = u.Age,
                    subscription = u.Subscription.ToString()
                });
            });

            // POST /api/users — adăugare utilizator nou
            app.MapPost("/api/users", async (HttpContext ctx) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<AddUserRequest>();
                if (body == null || string.IsNullOrWhiteSpace(body.Name))
                    return Results.BadRequest(new { error = "Numele este obligatoriu." });

                if (UserRegistry.ContainsKey(body.Name))
                    return Results.BadRequest(new { error = $"Utilizatorul '{body.Name}' există deja." });

                if (!Enum.TryParse<SubscriptionType>(body.Subscription, true, out var sub))
                    return Results.BadRequest(new { error = "Abonament invalid. Valori: Free, Standard, Premium" });

                var profile = new UserProfile(body.Name, body.Age, sub);
                UserRegistry[body.Name] = profile;
                PlatformManager.Instance.Log($"[Proxy] Utilizator adăugat: {body.Name}, {body.Age} ani, {sub}");

                return Results.Ok(new
                {
                    message = $"Utilizatorul '{body.Name}' a fost înregistrat.",
                    user = new { name = profile.Name, age = profile.Age, subscription = profile.Subscription.ToString() }
                });
            });

            // DELETE /api/users/{name} — ștergere utilizator
            app.MapDelete("/api/users/{name}", (string name) =>
            {
                if (!UserRegistry.Remove(name))
                    return Results.NotFound(new { error = $"Utilizatorul '{name}' nu există." });
                return Results.Ok(new { message = $"Utilizatorul '{name}' a fost șters." });
            });

            // POST /api/proxy/test-access — testare acces Proxy
            app.MapPost("/api/proxy/test-access", async (HttpContext ctx) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<ProxyTestRequest>();
                if (body == null) return Results.BadRequest(new { error = "Date invalide." });

                var steps = new List<string>();
                bool allowed = false;
                string reason = "";

                // Pas 1: Autentificare
                if (!UserRegistry.TryGetValue(body.UserName, out var profile))
                {
                    steps.Add($"[Pas 1] Autentificare: EȘUAT — '{body.UserName}' nu este înregistrat.");
                    reason = "Utilizator neautentificat.";
                    return Results.Ok(new { allowed, reason, steps });
                }
                steps.Add($"[Pas 1] Autentificare: OK — '{body.UserName}' găsit în registru.");

                // Găsim conținutul
                MediaContent? content = _movies.FirstOrDefault(m => m.Title.Equals(body.ContentTitle, StringComparison.OrdinalIgnoreCase)) as MediaContent
                    ?? _series.FirstOrDefault(s => s.Title.Equals(body.ContentTitle, StringComparison.OrdinalIgnoreCase)) as MediaContent
                    ?? _documentaries.FirstOrDefault(d => d.Title.Equals(body.ContentTitle, StringComparison.OrdinalIgnoreCase)) as MediaContent;

                if (content == null)
                {
                    steps.Add($"[Pas 2] Conținut: EȘUAT — '{body.ContentTitle}' nu există pe platformă.");
                    reason = "Conținut negăsit.";
                    return Results.Ok(new { allowed, reason, steps });
                }

                var rating = content.Rating;
                steps.Add($"[Pas 2] Conținut găsit: '{content.Title}' | Rating: {rating} | Tip: {content.GetType().Name}");

                // Pas 3: Verificare abonament
                bool subOk = profile.Subscription switch
                {
                    SubscriptionType.Premium  => true,
                    SubscriptionType.Standard => rating != ContentRating.R,
                    SubscriptionType.Free     => rating is ContentRating.G or ContentRating.PG,
                    _                         => false
                };

                if (!subOk)
                {
                    steps.Add($"[Pas 3] Abonament: REFUZAT — {profile.Subscription} nu permite conținut {rating}.");
                    reason = $"Abonamentul {profile.Subscription} nu permite conținut {rating}. Actualizează la Premium!";
                    return Results.Ok(new { allowed, reason, steps });
                }
                steps.Add($"[Pas 3] Abonament: OK — {profile.Subscription} permite conținut {rating}.");

                // Pas 4: Verificare vârstă
                int minAge = rating switch { ContentRating.R => 17, ContentRating.PG13 => 13, ContentRating.PG => 7, _ => 0 };
                bool ageOk = profile.Age >= minAge;

                if (!ageOk)
                {
                    steps.Add($"[Pas 4] Vârstă: REFUZAT — {profile.Age} ani, minimul pentru {rating} este {minAge} ani.");
                    reason = $"{profile.Name} ({profile.Age} ani) nu îndeplinește limita de vârstă {rating} ({minAge}+ ani).";
                    return Results.Ok(new { allowed, reason, steps });
                }
                steps.Add($"[Pas 4] Vârstă: OK — {profile.Age} ani ≥ {minAge} ani (limita {rating}).");

                allowed = true;
                reason = $"Acces permis! '{profile.Name}' ({profile.Subscription}, {profile.Age} ani) poate viziona '{content.Title}'.";
                steps.Add($"[Pas 5] ACCES PERMIS → Delegare la RealContentPlayer.Play()");

                PlatformManager.Instance.Log($"[Proxy API] Acces permis: {body.UserName} → {body.ContentTitle}");
                return Results.Ok(new { allowed, reason, steps });
            });

            // ================================================================
            // FLYWEIGHT PATTERN — Sesiuni streaming cu calitate partajată
            // ================================================================

            // GET /api/flyweight/sessions — listare sesiuni active
            app.MapGet("/api/flyweight/sessions", () =>
            {
                return new
                {
                    sessions = _activeSessions.Select(s => new
                    {
                        userName = s.UserName,
                        contentTitle = s.ContentTitle,
                        device = s.DeviceType,
                        quality = s.GetQualityLevel(),
                        qualityDetails = s.GetQualityInfo(),
                        startedAt = s.StartedAt.ToString("HH:mm:ss")
                    }),
                    poolStats = new
                    {
                        poolSize = StreamQualityFactory.GetPoolSize(),
                        totalRequests = StreamQualityFactory.GetTotalRequests(),
                        cacheHits = StreamQualityFactory.GetCacheHits(),
                        report = StreamQualityFactory.GetPoolReport()
                    },
                    qualityPool = StreamQualityFactory.GetPool().Select(kv => new
                    {
                        resolution = kv.Key,
                        description = kv.Value.GetDescription(),
                        usedBySessions = _activeSessions.Count(s => s.GetQualityLevel() == kv.Key)
                    })
                };
            });

            // POST /api/flyweight/sessions — adăugare sesiune nouă
            app.MapPost("/api/flyweight/sessions", async (HttpContext ctx) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<AddSessionRequest>();
                if (body == null || string.IsNullOrWhiteSpace(body.UserName))
                    return Results.BadRequest(new { error = "Date invalide." });

                var validQualities = StreamQualityFactory.GetPool().Keys.ToList();
                if (!validQualities.Contains(body.Quality))
                    return Results.BadRequest(new { error = $"Calitate invalidă. Valori: {string.Join(", ", validQualities)}" });

                int beforeHits = StreamQualityFactory.GetCacheHits();
                var session = new StreamingSession(body.UserName, body.ContentTitle, body.Device, body.Quality);
                _activeSessions.Add(session);
                bool wasCacheHit = StreamQualityFactory.GetCacheHits() > beforeHits;

                // Verificăm dacă există sesiuni cu aceeași calitate
                var sharedWith = _activeSessions
                    .Where(s => s != session && s.SharesQualityWith(session))
                    .Select(s => s.UserName)
                    .ToList();

                PlatformManager.Instance.Log($"[Flyweight API] Sesiune nouă: {body.UserName} → {body.Quality}");

                return Results.Ok(new
                {
                    message = $"Sesiune pornită pentru '{body.UserName}'.",
                    session = new
                    {
                        userName = session.UserName,
                        contentTitle = session.ContentTitle,
                        device = session.DeviceType,
                        quality = session.GetQualityLevel(),
                        qualityDetails = session.GetQualityInfo()
                    },
                    flyweightInfo = new
                    {
                        cacheHit = wasCacheHit,
                        message = wasCacheHit
                            ? $"Obiect StreamQuality '{body.Quality}' PARTAJAT din pool — zero alocare nouă!"
                            : $"Obiect StreamQuality '{body.Quality}' creat nou și adăugat în pool.",
                        sharedWith = sharedWith,
                        poolSize = StreamQualityFactory.GetPoolSize()
                    }
                });
            });

            // DELETE /api/flyweight/sessions — ștergere toate sesiunile
            app.MapDelete("/api/flyweight/sessions", () =>
            {
                int count = _activeSessions.Count;
                _activeSessions.Clear();
                return Results.Ok(new { message = $"Au fost șterse {count} sesiuni." });
            });

            // ================================================================
            // DECORATOR PATTERN — Notificări cu canale compuse dinamic
            // ================================================================

            // POST /api/decorator/notify — trimite notificare cu canale selectate
            app.MapPost("/api/decorator/notify", async (HttpContext ctx) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<NotifyRequest>();
                if (body == null || string.IsNullOrWhiteSpace(body.UserName) || string.IsNullOrWhiteSpace(body.Message))
                    return Results.BadRequest(new { error = "UserName și Message sunt obligatorii." });

                var channels = body.Channels ?? new List<string>();
                var outputs = new List<string>();
                var chainParts = new List<string> { "Base (Consolă)" };

                // Construim chain-ul și simulăm output-urile
                string timestamp = DateTime.Now.ToString("HH:mm:ss");

                // Base
                outputs.Add($"[{timestamp}] [Consolă] {body.UserName}: {body.Message}");

                if (channels.Contains("Email", StringComparer.OrdinalIgnoreCase))
                {
                    chainParts.Add("Email");
                    string subject = body.Message.Length > 50 ? body.Message[..47] + "..." : body.Message;
                    outputs.Add($"[EMAIL via smtp.streamzone.md] De la: noreply@streamzone.md → {body.UserName}@streamzone.md | Subiect: \"{subject}\"");
                }

                if (channels.Contains("SMS", StringComparer.OrdinalIgnoreCase))
                {
                    chainParts.Add("SMS");
                    int phoneHash = Math.Abs(body.UserName.GetHashCode()) % 9000 + 1000;
                    string smsText = body.Message.Length > 160 ? body.Message[..157] + "..." : body.Message;
                    outputs.Add($"[SMS via Orange Moldova] → +373-{phoneHash}: \"{smsText}\" ({smsText.Length} ch)");
                }

                if (channels.Contains("Push", StringComparer.OrdinalIgnoreCase))
                {
                    chainParts.Add("Push");
                    string icon = body.Message.Contains("expiră", StringComparison.OrdinalIgnoreCase) ? "⚠" :
                                  body.Message.Contains("nou", StringComparison.OrdinalIgnoreCase) ? "🆕" :
                                  body.Message.Contains("ofertă", StringComparison.OrdinalIgnoreCase) ? "🏷" : "🔔";
                    string priority = body.Message.Contains("expiră", StringComparison.OrdinalIgnoreCase) || body.Message.Contains("urgent", StringComparison.OrdinalIgnoreCase) ? "HIGH" : "NORMAL";
                    outputs.Add($"[PUSH via FCM/StreamZone] → {body.UserName} | {icon} \"{body.Message}\" | Prioritate: {priority}");
                }

                bool hasLogging = channels.Contains("Logging", StringComparer.OrdinalIgnoreCase);
                if (hasLogging)
                {
                    chainParts.Add("Logging");
                    outputs.Add($"[AUDIT LOG] {timestamp} | User: {body.UserName} | Canale: {string.Join("+", chainParts.SkipLast(1))} | Chars: {body.Message.Length}");
                }

                // Construim reprezentarea vizuală a chain-ului
                string chainVisual = string.Join("(", Enumerable.Reverse(chainParts)) + new string(')', chainParts.Count - 1);

                PlatformManager.Instance.Log($"[Decorator API] Notificare trimisă la {body.UserName} via {string.Join("+", chainParts)}");

                return Results.Ok(new
                {
                    chain = string.Join(" + ", chainParts),
                    chainVisual,
                    outputs,
                    channelCount = chainParts.Count
                });
            });

            // ================================================================
            // BRIDGE PATTERN — Redare media pe dispozitive diferite
            // ================================================================

            // POST /api/bridge/play — redare via bridge
            app.MapPost("/api/bridge/play", async (HttpContext ctx) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<BridgePlayRequest>();
                if (body == null) return Results.BadRequest(new { error = "Date invalide." });

                string deviceName, deviceCapabilities, emoji;
                (deviceName, deviceCapabilities, emoji) = body.Device?.ToLower() switch
                {
                    "mobile"  => ("Mobile (Android)", "Max 1080p | H.264 | Touch | Economie baterie", "📱"),
                    "desktop" => ("Desktop (Firefox)", "Max 4K | H.265 | Mouse+Keyboard | Performanță maximă", "🖥"),
                    "smarttv" => ("Smart TV (LG)", "Max 4K | HDR10 | AV1 | Dolby Atmos | Telecomandă", "📺"),
                    "tablet"  => ("Tabletă (Samsung Galaxy Tab)", "Max 1080p | H.265 | Touch | Ecran mare", "⬜"),
                    _         => ("Unknown Device", "Necunoscut", "❓")
                };

                string playerType, mediaAction;
                (playerType, mediaAction) = body.PlayerType?.ToLower() switch
                {
                    "video" => ("Video Player (Film / Serial)", "RenderVideo()"),
                    "audio" => ("Audio Player", "RenderAudio()"),
                    "live"  => ("Live Stream Player", "RenderLiveStream()"),
                    _       => ("Unknown Player", "Render()")
                };

                // Calitate adaptată pentru dispozitiv
                string effectiveQuality = body.Quality;
                string qualityNote = "";
                if (body.Device?.ToLower() is "mobile" or "tablet" && body.Quality is "4K" or "4KUHD")
                {
                    effectiveQuality = "1080p";
                    qualityNote = $"(adaptat din {body.Quality} → {effectiveQuality} pentru {deviceName})";
                }

                var steps = new List<string>
                {
                    $"[Bridge] Abstracție: {playerType}",
                    $"[Bridge] Implementare: {deviceName}",
                    $"[Bridge] Metodă delegată: {mediaAction}",
                    $"[Bridge] Calitate: {effectiveQuality} {qualityNote}".Trim()
                };

                string output = body.PlayerType?.ToLower() switch
                {
                    "audio" => $"{emoji} [{deviceName}] Redare audio: '{body.ContentTitle}' | Format: FLAC | Difuzor/Căști",
                    "live"  => $"{emoji} [{deviceName}] LIVE: '{body.ContentTitle}' | HD | Latență adaptivă",
                    _       => $"{emoji} [{deviceName}] Redare video: '{body.ContentTitle}' @ {effectiveQuality} | {deviceCapabilities.Split('|')[0].Trim()}"
                };

                steps.Add($"[Bridge] Output: {output}");

                PlatformManager.Instance.Log($"[Bridge API] {playerType} → {deviceName}: {body.ContentTitle}");

                return Results.Ok(new
                {
                    player = playerType,
                    device = deviceName,
                    deviceCapabilities,
                    bridgeCall = $"{playerType}.Play() → {deviceName}.{mediaAction}",
                    steps,
                    output,
                    keyInsight = $"Putem schimba '{deviceName}' cu orice alt dispozitiv la RUNTIME fără a modifica {playerType}."
                });
            });

            // ================================================================
            // STRATEGY PATTERN — Recomandări cu algoritmi interschimbabili
            // ================================================================

            // GET /api/strategy/recommend?strategy=toprated&max=5
            app.MapGet("/api/strategy/recommend", (string? strategy, string? genre, int? max) =>
            {
                var allContent = GetAllContent();
                int maxResults = max ?? 5;

                IRecommendationStrategy selectedStrategy = strategy?.ToLower() switch
                {
                    "mostviewed"   => new MostViewedStrategy(),
                    "bygenre"      => new ByGenreStrategy(
                                          Enum.TryParse<Genre>(genre, true, out var g) ? g : Genre.Drama),
                    "shortcontent" => new ShortContentStrategy(),
                    _              => new TopRatedStrategy()
                };

                var context = new RecommendationContext(selectedStrategy);
                var results = context.GetRecommendations(allContent, maxResults);

                return Results.Ok(new
                {
                    strategy = context.CurrentStrategy,
                    maxResults,
                    count = results.Count,
                    availableStrategies = new[] { "toprated", "mostviewed", "bygenre", "shortcontent" },
                    recommendations = results.Select(c => new
                    {
                        title = c.Title,
                        type = c.GetType().Name,
                        genre = c.Genre.ToString(),
                        averageRating = c.AverageRating,
                        views = c.ViewsCount,
                        duration = c.GetDuration()
                    })
                });
            });

            // ================================================================
            // OBSERVER PATTERN — Abonare/notificare conținut nou
            // ================================================================

            // GET /api/observer/subscribers
            app.MapGet("/api/observer/subscribers", () =>
            {
                return Results.Ok(new
                {
                    observerCount = _contentPublisher.ObserverCount,
                    eventLog = _contentPublisher.EventLog
                });
            });

            // POST /api/observer/subscribe — abonare utilizator
            app.MapPost("/api/observer/subscribe", async (HttpContext ctx) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<ObserverSubscribeRequest>();
                if (body == null || string.IsNullOrWhiteSpace(body.UserName))
                    return Results.BadRequest(new { error = "UserName este obligatoriu." });

                // Creăm observer dacă nu există deja
                if (!_observersByUser.ContainsKey(body.UserName))
                {
                    var obs = new UserNotificationObserver(body.UserName);
                    _observersByUser[body.UserName] = obs;
                    _contentPublisher.Subscribe(obs);
                }

                // Stocăm email-ul pentru notificări reale (dacă a fost trimis)
                if (!string.IsNullOrWhiteSpace(body.Email))
                    _observerEmails[body.UserName] = body.Email.Trim();

                return Results.Ok(new
                {
                    message = $"'{body.UserName}' s-a abonat la notificări.",
                    observerCount = _contentPublisher.ObserverCount,
                    emailRegistered = _observerEmails.ContainsKey(body.UserName)
                });
            });

            // POST /api/observer/unsubscribe
            app.MapPost("/api/observer/unsubscribe", async (HttpContext ctx) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<ObserverSubscribeRequest>();
                if (body == null || string.IsNullOrWhiteSpace(body.UserName))
                    return Results.BadRequest(new { error = "UserName este obligatoriu." });

                if (_observersByUser.TryGetValue(body.UserName, out var obs))
                {
                    _contentPublisher.Unsubscribe(obs);
                    _observersByUser.Remove(body.UserName);
                    _observerEmails.Remove(body.UserName);
                    return Results.Ok(new { message = $"'{body.UserName}' s-a dezabonat.", observerCount = _contentPublisher.ObserverCount });
                }
                return Results.BadRequest(new { error = $"'{body.UserName}' nu era abonat." });
            });

            // POST /api/observer/publish — simulează adăugare conținut nou + trimite email REAL
            app.MapPost("/api/observer/publish", async (HttpContext ctx, IEmailService emailService) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<ObserverPublishRequest>();
                if (body == null || string.IsNullOrWhiteSpace(body.Title))
                    return Results.BadRequest(new { error = "Title este obligatoriu." });

                if (!Enum.TryParse<Genre>(body.Genre ?? "Drama", true, out var genre))
                    genre = Genre.Drama;
                if (!Enum.TryParse<ContentRating>(body.Rating ?? "PG", true, out var rating))
                    rating = ContentRating.PG;

                var newMovie = new Movie(body.Title, body.Description ?? "", genre, rating, 90, "Regizor Necunoscut");
                _contentPublisher.NotifyNewContent(newMovie);

                PlatformManager.Instance.Log($"[Observer API] Conținut publicat: {body.Title}");

                // ── Trimitere email REAL la fiecare abonat care are email salvat ─
                int emailsSent = 0, emailsFailed = 0;
                var recipients = new List<string>();

                if (_observerEmails.Count > 0)
                {
                    var subject = $"🎬 Lansare nouă pe StreamZone: {body.Title}";
                    var html = BuildNewContentEmailHtml(body.Title, body.Description ?? "", genre.ToString(), rating.ToString());

                    foreach (var (userName, email) in _observerEmails)
                    {
                        var ok = await emailService.SendAsync(email, subject, html);
                        if (ok) { emailsSent++; recipients.Add(email); }
                        else emailsFailed++;
                    }
                }

                return Results.Ok(new
                {
                    message = $"Conținut '{body.Title}' publicat. {_contentPublisher.ObserverCount} observatori notificați.",
                    eventLog = _contentPublisher.EventLog,
                    email = new
                    {
                        provider = emailService.ProviderName,
                        isReal = emailService.IsRealProvider,
                        sent = emailsSent,
                        failed = emailsFailed,
                        recipients
                    }
                });
            });

            // ================================================================
            // COMMAND PATTERN — Watchlist cu Undo/Redo
            // ================================================================

            // GET /api/command/watchlist/{user}
            app.MapGet("/api/command/watchlist/{user}", (string user) =>
            {
                var watchlist = GetOrCreateWatchlist(user);
                var history = GetOrCreateHistory(user);
                return Results.Ok(new
                {
                    user,
                    items = watchlist.Items,
                    ratings = watchlist.Ratings,
                    canUndo = history.CanUndo,
                    canRedo = history.CanRedo,
                    log = history.Log
                });
            });

            // POST /api/command/watchlist/{user}/add
            app.MapPost("/api/command/watchlist/{user}/add", async (string user, HttpContext ctx) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<WatchlistItemRequest>();
                if (body == null || string.IsNullOrWhiteSpace(body.Title))
                    return Results.BadRequest(new { error = "Title obligatoriu." });

                var watchlist = GetOrCreateWatchlist(user);
                var history = GetOrCreateHistory(user);
                history.Execute(new AddToWatchlistCommand(watchlist, body.Title));

                return Results.Ok(new { message = $"'{body.Title}' adăugat.", items = watchlist.Items, canUndo = history.CanUndo });
            });

            // POST /api/command/watchlist/{user}/remove
            app.MapPost("/api/command/watchlist/{user}/remove", async (string user, HttpContext ctx) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<WatchlistItemRequest>();
                if (body == null || string.IsNullOrWhiteSpace(body.Title))
                    return Results.BadRequest(new { error = "Title obligatoriu." });

                var watchlist = GetOrCreateWatchlist(user);
                var history = GetOrCreateHistory(user);
                history.Execute(new RemoveFromWatchlistCommand(watchlist, body.Title));

                return Results.Ok(new { message = $"'{body.Title}' eliminat.", items = watchlist.Items, canUndo = history.CanUndo });
            });

            // POST /api/command/watchlist/{user}/rate
            app.MapPost("/api/command/watchlist/{user}/rate", async (string user, HttpContext ctx) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<RateRequest>();
                if (body == null) return Results.BadRequest(new { error = "Date invalide." });

                var watchlist = GetOrCreateWatchlist(user);
                var history = GetOrCreateHistory(user);
                history.Execute(new RateContentCommand(watchlist, body.Title, body.Rating));

                return Results.Ok(new { message = $"Rating {body.Rating}/5 acordat pentru '{body.Title}'.", ratings = watchlist.Ratings, canUndo = history.CanUndo });
            });

            // POST /api/command/watchlist/{user}/undo
            app.MapPost("/api/command/watchlist/{user}/undo", (string user) =>
            {
                var watchlist = GetOrCreateWatchlist(user);
                var history = GetOrCreateHistory(user);
                var undone = history.Undo();
                if (undone == null) return Results.BadRequest(new { error = "Nimic de anulat." });
                return Results.Ok(new { message = $"Anulat: {undone}", items = watchlist.Items, ratings = watchlist.Ratings, canUndo = history.CanUndo, canRedo = history.CanRedo });
            });

            // POST /api/command/watchlist/{user}/redo
            app.MapPost("/api/command/watchlist/{user}/redo", (string user) =>
            {
                var watchlist = GetOrCreateWatchlist(user);
                var history = GetOrCreateHistory(user);
                var redone = history.Redo();
                if (redone == null) return Results.BadRequest(new { error = "Nimic de refăcut." });
                return Results.Ok(new { message = $"Refăcut: {redone}", items = watchlist.Items, ratings = watchlist.Ratings, canUndo = history.CanUndo, canRedo = history.CanRedo });
            });

            // ================================================================
            // MEMENTO PATTERN — Salvare/Restaurare stare sesiune
            // ================================================================

            // GET /api/memento/session/{user}
            app.MapGet("/api/memento/session/{user}", (string user) =>
            {
                var (state, hist) = GetOrCreateSession(user);
                return Results.Ok(new
                {
                    user,
                    current = state.ToString(),
                    genreFilter = state.GenreFilter,
                    sortBy = state.SortBy,
                    searchQuery = state.SearchQuery,
                    currentPage = state.CurrentPage,
                    canUndo = hist.CanUndo,
                    canRedo = hist.CanRedo,
                    historyCount = hist.HistoryCount,
                    history = hist.GetHistory()
                });
            });

            // POST /api/memento/session/{user}/save
            app.MapPost("/api/memento/session/{user}/save", async (string user, HttpContext ctx) =>
            {
                var body = await ctx.Request.ReadFromJsonAsync<SessionStateRequest>();
                if (body == null) return Results.BadRequest(new { error = "Date invalide." });

                var (state, hist) = GetOrCreateSession(user);
                hist.Save(state.SaveState()); // salva starea curentă înainte de schimbare
                if (body.GenreFilter != null) state.GenreFilter = body.GenreFilter;
                if (body.SortBy != null) state.SortBy = body.SortBy;
                if (body.SearchQuery != null) state.SearchQuery = body.SearchQuery;
                if (body.CurrentPage.HasValue) state.CurrentPage = body.CurrentPage.Value;

                return Results.Ok(new
                {
                    message = "Stare salvată și aplicată.",
                    current = state.ToString(),
                    canUndo = hist.CanUndo,
                    historyCount = hist.HistoryCount
                });
            });

            // POST /api/memento/session/{user}/undo
            app.MapPost("/api/memento/session/{user}/undo", (string user) =>
            {
                var (state, hist) = GetOrCreateSession(user);
                var prev = hist.Undo();
                if (prev == null) return Results.BadRequest(new { error = "Nu există stare anterioară." });
                state.RestoreState(prev);
                return Results.Ok(new { message = "Stare restaurată (Undo).", current = state.ToString(), canUndo = hist.CanUndo, canRedo = hist.CanRedo });
            });

            // POST /api/memento/session/{user}/redo
            app.MapPost("/api/memento/session/{user}/redo", (string user) =>
            {
                var (state, hist) = GetOrCreateSession(user);
                var next = hist.Redo();
                if (next == null) return Results.BadRequest(new { error = "Nu există stare ulterioară." });
                state.RestoreState(next);
                return Results.Ok(new { message = "Stare restaurată (Redo).", current = state.ToString(), canUndo = hist.CanUndo, canRedo = hist.CanRedo });
            });

            // ================================================================
            // ITERATOR PATTERN — Navigare colecție conținut
            // ================================================================

            // GET /api/iterator?type=genre&genre=Drama&top=5
            app.MapGet("/api/iterator", (string? type, string? genre, int? top) =>
            {
                var allContent = GetAllContent();
                var collection = new ContentCollection(allContent);

                IContentIterator iterator;
                string iteratorType;

                if (type?.ToLower() == "genre" && Enum.TryParse<Genre>(genre, true, out var g))
                {
                    iterator = collection.CreateGenreIterator(g);
                    iteratorType = $"GenreIterator({g})";
                }
                else if (type?.ToLower() == "toprated")
                {
                    iterator = collection.CreateTopRatedIterator(top ?? 5);
                    iteratorType = $"TopRatedIterator(top={top ?? 5})";
                }
                else
                {
                    iterator = collection.CreateIterator();
                    iteratorType = "SequentialIterator";
                }

                var items = new List<object>();
                int rank = 1;
                while (iterator.HasNext())
                {
                    var item = iterator.Next();
                    items.Add(new
                    {
                        rank = rank++,
                        title = item.Title,
                        type = item.GetType().Name,
                        genre = item.Genre.ToString(),
                        averageRating = item.AverageRating,
                        duration = item.GetDuration(),
                        views = item.ViewsCount
                    });
                }

                return Results.Ok(new
                {
                    iteratorType,
                    totalCount = iterator.TotalCount,
                    availableTypes = new[] { "sequential", "genre", "toprated" },
                    items
                });
            });
        }

        // ── Lab 6 helpers ─────────────────────────────────────────────────────
        private static readonly Dictionary<string, UserNotificationObserver> _observersByUser = new();

        // Email-ul fiecărui observer (key = userName) — folosit pentru notificări reale
        private static readonly Dictionary<string, string> _observerEmails = new();

        private static Watchlist GetOrCreateWatchlist(string user)
        {
            if (!_watchlists.ContainsKey(user))
                _watchlists[user] = new Watchlist(user);
            return _watchlists[user];
        }

        private static CommandHistory GetOrCreateHistory(string user)
        {
            if (!_commandHistories.ContainsKey(user))
                _commandHistories[user] = new CommandHistory();
            return _commandHistories[user];
        }

        private static (UserSessionState State, SessionHistory History) GetOrCreateSession(string user)
        {
            if (!_sessions.ContainsKey(user))
                _sessions[user] = (new UserSessionState(user), new SessionHistory());
            return _sessions[user];
        }

        // ── Email HTML template pentru conținut nou ──────────────────────────
        private static string BuildNewContentEmailHtml(string title, string description, string genre, string rating)
        {
            var safeTitle = System.Net.WebUtility.HtmlEncode(title);
            var safeDesc  = System.Net.WebUtility.HtmlEncode(description);
            var safeGenre = System.Net.WebUtility.HtmlEncode(genre);
            var encodedTitle = Uri.EscapeDataString(title);

            return $@"<!DOCTYPE html>
<html lang=""ro""><head><meta charset=""UTF-8""></head>
<body style=""margin:0;padding:0;font-family:Arial,Helvetica,sans-serif;background:#1a0a0e;color:#f5f0f1;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#1a0a0e;padding:32px 0;"">
    <tr><td align=""center"">
      <table width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background:linear-gradient(160deg,#321520,#2a1118);border:1px solid rgba(212,168,83,0.3);border-radius:12px;padding:36px 32px;"">
        <tr><td>
          <div style=""font-family:Georgia,serif;font-size:28px;font-weight:800;letter-spacing:2px;background:linear-gradient(135deg,#c45567,#d4a853);-webkit-background-clip:text;-webkit-text-fill-color:transparent;margin-bottom:24px;"">STREAMZONE</div>
          <div style=""display:inline-block;padding:6px 14px;background:rgba(212,168,83,0.18);color:#d4a853;border-radius:999px;font-size:11px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;margin-bottom:14px;"">🎬 Lansare nouă</div>
          <h1 style=""font-family:Georgia,serif;font-size:32px;font-weight:700;color:#f5f0f1;margin:0 0 12px;"">{safeTitle}</h1>
          <div style=""color:#9a8a8e;font-size:13px;margin-bottom:18px;"">
            <span style=""padding:3px 10px;background:rgba(123,46,58,0.4);border-radius:4px;color:#e0d5d8;margin-right:6px;"">{safeGenre}</span>
            <span style=""padding:3px 10px;background:rgba(123,46,58,0.4);border-radius:4px;color:#e0d5d8;"">{rating}</span>
          </div>
          <p style=""color:#e0d5d8;font-size:15px;line-height:1.6;margin:0 0 28px;"">{safeDesc}</p>
          <a href=""http://localhost:5000/watch.html?title={encodedTitle}"" style=""display:inline-block;padding:14px 32px;background:linear-gradient(135deg,#7b2e3a,#a0404d);color:#fff;text-decoration:none;border-radius:30px;font-weight:600;letter-spacing:1px;text-transform:uppercase;font-size:13px;"">▶ Vizionează acum</a>
          <hr style=""border:none;border-top:1px solid rgba(196,85,103,0.18);margin:32px 0 18px;"">
          <p style=""color:#6b5a5f;font-size:11px;line-height:1.6;margin:0;"">Primești acest email pentru că ești abonat la lansările noi pe StreamZone (Observer pattern). Pentru a te dezabona, intră în contul tău.</p>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body></html>";
        }

        // ── Request DTOs ──────────────────────────────────────────────────────
        private record AddUserRequest(string Name, int Age, string Subscription);
        private record ProxyTestRequest(string UserName, string ContentTitle);
        private record AddSessionRequest(string UserName, string ContentTitle, string Device, string Quality);
        private record NotifyRequest(string UserName, string Message, List<string> Channels);
        private record BridgePlayRequest(string PlayerType, string Device, string ContentTitle, string Quality);
        private record ObserverSubscribeRequest(string UserName, string? Email);
        private record ObserverPublishRequest(string Title, string? Description, string? Genre, string? Rating);
        private record WatchlistItemRequest(string Title);
        private record RateRequest(string Title, double Rating);
        private record SessionStateRequest(string? GenreFilter, string? SortBy, string? SearchQuery, int? CurrentPage);
    }
}
