using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using StreamingPlatform.Api;
using AuthService = StreamingPlatform.Auth.AuthService;
using StreamZoneDbContext = StreamingPlatform.Auth.StreamZoneDbContext;
using StreamingPlatform.Flyweight;
using StreamingPlatform.Decorator;
using StreamingPlatform.Bridge;
using StreamingPlatform.Proxy;
using StreamingPlatform.Models;
using StreamingPlatform.Behavioral.Strategy;
using StreamingPlatform.Behavioral.Observer;
using StreamingPlatform.Behavioral.Command;
using StreamingPlatform.Behavioral.Memento;
using StreamingPlatform.Behavioral.Iterator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace StreamingPlatform
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Creăm web server-ul mai întâi — avem nevoie de DbContext pentru LoadData
            var builder = WebApplication.CreateBuilder(args);

            // ── Conectare bază de date StreamZoneDB (SQL Server) ─────────────
            var connectionString = builder.Configuration.GetConnectionString("StreamZoneDB")
                ?? "Server=localhost;Database=StreamZoneDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;";
            builder.Services.AddDbContext<StreamZoneDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddScoped<AuthService>();

            // ── Email service (real SMTP dacă e configurat, altfel mock) ─────
            var emailProvider = builder.Configuration["Email:Provider"] ?? "Mock";
            var smtpUser = builder.Configuration["Email:Smtp:Username"];
            if (emailProvider == "Smtp" && !string.IsNullOrWhiteSpace(smtpUser))
                builder.Services.AddSingleton<StreamingPlatform.Services.IEmailService, StreamingPlatform.Services.SmtpEmailService>();
            else
                builder.Services.AddSingleton<StreamingPlatform.Services.IEmailService, StreamingPlatform.Services.MockEmailService>();

            var app = builder.Build();

            // ── Verificare conectare DB + încărcare conținut (hibrid: BD ↔ hardcoded) ─
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StreamZoneDbContext>();
                try
                {
                    if (db.Database.CanConnect())
                    {
                        Console.WriteLine("  ✅ Bază de date StreamZoneDB conectată.");
                        ApiEndpoints.LoadData(db); // citește din BD sau seedează BD
                    }
                    else
                    {
                        Console.WriteLine("  ⚠ Nu se poate conecta la StreamZoneDB — folosesc date hardcoded.");
                        ApiEndpoints.LoadData(null);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ⚠ Eroare conexiune DB: {ex.Message} — folosesc date hardcoded.");
                    ApiEndpoints.LoadData(null);
                }
            }

            // ── Demo-uri pattern-uri (folosesc listele in-memory populate de LoadData) ─
            DemoFlyweight();
            DemoDecorator();
            DemoBridge();
            DemoProxy();
            DemoStrategy();
            DemoObserver();
            DemoCommand();
            DemoMemento();
            DemoIterator();

            // Servește fișierele statice din folderul UI/
            var uiPath = Path.Combine(Directory.GetCurrentDirectory(), "UI");
            if (Directory.Exists(uiPath))
            {
                 // Servește index.html ca pagină principală
                app.UseDefaultFiles(new DefaultFilesOptions
                {
                    FileProvider = new PhysicalFileProvider(uiPath),
                    DefaultFileNames = new List<string> { "index.html" }
                });

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(uiPath),
                    RequestPath = ""
                });

            }

            // Înregistrează endpoint-urile API
            ApiEndpoints.MapEndpoints(app);
            AuthEndpoints.MapAuthEndpoints(app);
            AccountEndpoints.MapAccountEndpoints(app);

            // Redirecționează "/" către index.html
            app.MapGet("/", (HttpContext context) =>
            {
                context.Response.Redirect("/index.html");
                return Task.CompletedTask;
            });

            Console.WriteLine("  ✅ API pornit cu succes!");
            Console.WriteLine("  ┌─────────────────────────────────────────┐");
            Console.WriteLine("  │  Deschide în browser:                   │");
            Console.WriteLine("  │  http://localhost:5000                   │");
            Console.WriteLine("  │                                         │");
            Console.WriteLine("  │  API Endpoints:                         │");
            Console.WriteLine("  │  http://localhost:5000/api/movies        │");
            Console.WriteLine("  │  http://localhost:5000/api/series        │");
            Console.WriteLine("  │  http://localhost:5000/api/documentaries │");
            Console.WriteLine("  │  http://localhost:5000/api/content       │");
            Console.WriteLine("  │  http://localhost:5000/api/genres        │");
            Console.WriteLine("  │  http://localhost:5000/api/stats         │");
            Console.WriteLine("  │  http://localhost:5000/api/search?q=dark │");
            Console.WriteLine("  │  http://localhost:5000/api/ratings/Inception │");
            Console.WriteLine("  │                                         │");
            Console.WriteLine("  │  Auth (DB):                             │");
            Console.WriteLine("  │  POST /api/auth/register                │");
            Console.WriteLine("  │  POST /api/auth/login                   │");
            Console.WriteLine("  │  POST /api/auth/refresh                 │");
            Console.WriteLine("  │  POST /api/auth/logout                  │");
            Console.WriteLine("  │  GET  /api/accounts/{id}/profiles       │");
            Console.WriteLine("  │  GET  /api/db/health                    │");
            Console.WriteLine("  │                                         │");
            Console.WriteLine("  │  Apasă Ctrl+C pentru a opri serverul    │");
            Console.WriteLine("  └─────────────────────────────────────────┘");
            Console.WriteLine();

            // Deschide browser-ul automat
System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
{
    FileName = "http://localhost:5000",
    UseShellExecute = true
});

app.Run("http://localhost:5000");

        }

        // ============================================================
        // LAB 5 — DEMO FLYWEIGHT PATTERN
        // ============================================================
        static void DemoFlyweight()
        {
            Console.WriteLine("┌──────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  LAB 5 — FLYWEIGHT PATTERN: Partajarea calităților stream    │");
            Console.WriteLine("└──────────────────────────────────────────────────────────────┘");

            Console.WriteLine("\n  Scenariu: 6 utilizatori pornesc sesiuni de streaming.");
            Console.WriteLine("  Unii aleg aceeași calitate — Flyweight garantează că");
            Console.WriteLine("  acele obiecte StreamQuality sunt PARTAJATE (nu duplicate).\n");

            // Creăm 6 sesiuni cu calități variate (unele repetate)
            var session1 = new StreamingSession("Ion Cojocaru",  "Carbon",            "Mobile",   "1080p");
            var session2 = new StreamingSession("Maria Rusu",    "Lăutarii",          "Smart TV", "4K");
            var session3 = new StreamingSession("Andrei Lupan",  "Plaha",             "Desktop",  "1080p"); // Aceeași calitate ca session1!
            var session4 = new StreamingSession("Elena Moraru",  "Moldova: Inima de Vin", "Tabletă", "720p");
            var session5 = new StreamingSession("Vlad Ciobanu",  "Abis",              "Mobile",   "4K");   // Aceeași calitate ca session2!
            var session6 = new StreamingSession("Ana Balan",     "Chișinău — Memorii","Desktop",  "720p"); // Aceeași ca session4!

            Console.WriteLine("  Sesiuni active:");
            Console.WriteLine("  " + session1.Play());
            Console.WriteLine("  " + session2.Play());
            Console.WriteLine("  " + session3.Play());
            Console.WriteLine("  " + session4.Play());
            Console.WriteLine("  " + session5.Play());
            Console.WriteLine("  " + session6.Play());

            // Demonstrăm că sesiunile cu aceeași calitate partajează ACELAȘI obiect
            Console.WriteLine("\n  Verificare partajare (ReferenceEquals):");
            bool s1s3Same = session1.SharesQualityWith(session3);
            bool s2s5Same = session2.SharesQualityWith(session5);
            bool s4s6Same = session4.SharesQualityWith(session6);
            bool s1s2Same = session1.SharesQualityWith(session2);

            Console.WriteLine($"  session1 (1080p) și session3 (1080p) → ACELAȘI obiect: {s1s3Same}");
            Console.WriteLine($"  session2 (4K)    și session5 (4K)    → ACELAȘI obiect: {s2s5Same}");
            Console.WriteLine($"  session4 (720p)  și session6 (720p)  → ACELAȘI obiect: {s4s6Same}");
            Console.WriteLine($"  session1 (1080p) și session2 (4K)    → ACELAȘI obiect: {s1s2Same}");

            // Raport pool
            Console.WriteLine("\n  Raport Flyweight Factory:");
            Console.WriteLine($"  {StreamQualityFactory.GetPoolReport()}");

            Console.WriteLine("\n  Calități disponibile în pool:");
            foreach (var kv in StreamQualityFactory.GetPool())
                Console.WriteLine($"    [{kv.Key}] {kv.Value.GetDescription()}");

            Console.WriteLine("\n  Concluzie Flyweight:");
            Console.WriteLine("  6 sesiuni → doar 3 obiecte StreamQuality unice în memorie.");
            Console.WriteLine("  La 1.000.000 sesiuni cu 6 calități → tot 6 obiecte! Zero alocare extra.\n");
        }

        // ============================================================
        // LAB 5 — DEMO DECORATOR PATTERN
        // ============================================================
        static void DemoDecorator()
        {
            Console.WriteLine("\n  Scenariu: Notificări pentru 3 tipuri de evenimente pe platformă.");
            Console.WriteLine("  Fiecare eveniment necesită combinații diferite de canale.\n");

            // ── Caz 1: Stream pornit → DOAR consolă (fără decoratoare) ──────────
            Console.WriteLine("  [Caz 1] Stream pornit — doar notificare internă:");
            IStreamNotification simpleNotif = new BaseStreamNotification();
            simpleNotif.Send("Ion Cojocaru", "Acum vizionezi: 'Carbon'");
            Console.WriteLine($"    Canale active: {simpleNotif.GetChannels()}\n");

            // ── Caz 2: Conținut nou → Email + Push ───────────────────────────────
            Console.WriteLine("  [Caz 2] Conținut nou adăugat — Email + Push:");
            IStreamNotification newContentNotif =
                new PushNotificationDecorator(
                    new EmailNotificationDecorator(
                        new BaseStreamNotification()));
            newContentNotif.Send("Maria Rusu", "Film nou adăugat: 'Puterea Probabilității'!");
            Console.WriteLine($"    Canale active: {newContentNotif.GetChannels()}\n");

            // ── Caz 3: Abonament expiră → Email + SMS + Push ─────────────────────
            Console.WriteLine("  [Caz 3] Abonament expiră — Email + SMS + Push:");
            IStreamNotification urgentNotif =
                new PushNotificationDecorator(
                    new SmsNotificationDecorator(
                        new EmailNotificationDecorator(
                            new BaseStreamNotification())));
            urgentNotif.Send("Andrei Lupan", "Abonamentul tău Premium expiră în 3 zile. Reînnoi-l acum!");
            Console.WriteLine($"    Canale active: {urgentNotif.GetChannels()}\n");

            // ── Caz 4: Ofertă specială + Audit complet ─────────────────────────
            Console.WriteLine("  [Caz 4] Ofertă specială — SMS + Push + Audit logging:");
            var auditDecorator = new LoggingNotificationDecorator(
                new PushNotificationDecorator(
                    new SmsNotificationDecorator(
                        new BaseStreamNotification())));
            auditDecorator.Send("Elena Moraru", "Ofertă exclusivă: 3 luni Premium cu 50% reducere!");
            Console.WriteLine($"    Canale active: {auditDecorator.GetChannels()}");
            Console.WriteLine(auditDecorator.GetAuditReport());

            Console.WriteLine("  Concluzie Decorator:");
            Console.WriteLine("  4 combinații → 0 clase noi de combinații.");
            Console.WriteLine("  Se compun dinamic: new Push(new Sms(new Email(new Base())))\n");
        }

        // ============================================================
        // LAB 5 — DEMO BRIDGE PATTERN
        // ============================================================
        static void DemoBridge()
        {
            Console.WriteLine("┌──────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  LAB 5 — BRIDGE PATTERN: Media pe dispozitive multiple       │");
            Console.WriteLine("└──────────────────────────────────────────────────────────────┘");

            Console.WriteLine("\n  Scenariu: Același conținut redat pe dispozitive diferite.");
            Console.WriteLine("  Putem schimba dispozitivul la RUNTIME fără a schimba playerul.\n");

            // ── Dispozitive (Implementări) ────────────────────────────────────────
            IDeviceRenderer mobile  = new MobileRenderer("Android");
            IDeviceRenderer desktop = new DesktopRenderer("Firefox");
            IDeviceRenderer tv      = new SmartTVRenderer("LG");
            IDeviceRenderer tablet  = new TabletRenderer("Samsung Galaxy Tab");

            // ── Caz 1: VideoPlayer — film pe Mobile ──────────────────────────────
            Console.WriteLine("  [Caz 1] Film pe Mobile:");
            var videoPlayer = new VideoMediaPlayer(mobile, subtitles: true);
            Console.WriteLine($"    Player: {videoPlayer.GetPlayerType()} | Dispozitiv: {videoPlayer.GetCurrentDevice()}");
            videoPlayer.Play("Carbon", "1080p");

            // ── Caz 2: Aceeași abstractie, dispozitiv schimbat la runtime ─────────
            Console.WriteLine("\n  [Caz 2] Același VideoPlayer, comutare la Smart TV:");
            videoPlayer.SwitchDevice(tv); // Bridge — schimbăm implementarea dinamic!
            Console.WriteLine($"    Dispozitiv curent: {videoPlayer.GetCurrentDevice()}");
            Console.WriteLine($"    Capabilități: {videoPlayer.GetDeviceCapabilities()}");
            videoPlayer.Play("Lăutarii", "4K");

            // ── Caz 3: Episod serial pe Tabletă ──────────────────────────────────
            Console.WriteLine("\n  [Caz 3] Episod serial pe Tabletă:");
            var tabletVideo = new VideoMediaPlayer(tablet, subtitles: false);
            tabletVideo.PlayEpisode("Plaha", 1, 3, "1080p");

            // ── Caz 4: AudioPlayer — coloană sonoră pe Desktop ───────────────────
            Console.WriteLine("\n  [Caz 4] Coloană sonoră pe Desktop:");
            var audioPlayer = new AudioMediaPlayer(desktop, format: "FLAC");
            Console.WriteLine($"    Player: {audioPlayer.GetPlayerType()}");
            audioPlayer.PlaySoundtrack("Lăutarii");

            // ── Caz 5: Live Stream — premieră pe TV ──────────────────────────────
            Console.WriteLine("\n  [Caz 5] Premieră live pe Smart TV:");
            var livePlayer = new LiveStreamPlayer(tv, initialViewers: 12500, isHD: true);
            Console.WriteLine($"    Player: {livePlayer.GetPlayerType()} | Dispozitiv: {livePlayer.GetCurrentDevice()}");
            livePlayer.JoinPremiere("Carbon");

            // ── Caz 6: Live comutare la Mobile ────────────────────────────────────
            Console.WriteLine("\n  [Caz 6] Același Live Stream, comutare la Mobile (continuare pe telefon):");
            livePlayer.SwitchDevice(mobile);
            livePlayer.Play("Carbon — LIVE", "720p");

            Console.WriteLine("\n  Concluzie Bridge:");
            Console.WriteLine("  3 tipuri media × 4 dispozitive = 7 clase (nu 12).");
            Console.WriteLine("  Schimbi dispozitivul la runtime cu SwitchDevice() — fără a recrea playerul.\n");
        }

        // ============================================================
        // LAB 5 — DEMO PROXY PATTERN
        // ============================================================
        static void DemoProxy()
        {
            Console.WriteLine("┌──────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  LAB 5 — PROXY PATTERN: Control acces și lazy loading        │");
            Console.WriteLine("└──────────────────────────────────────────────────────────────┘");

            Console.WriteLine("\n  Scenariu A — Protection Proxy: Control acces la film R-rated.\n");

            // Registrul utilizatorilor (gestionat de Proxy, nu de MediaContent)
            var userRegistry = new Dictionary<string, UserProfile>
            {
                ["Ion Cojocaru"] = new UserProfile("Ion Cojocaru",  25, SubscriptionType.Premium),
                ["Maria Rusu"]   = new UserProfile("Maria Rusu",    15, SubscriptionType.Standard),
                ["Andrei Lupan"] = new UserProfile("Andrei Lupan",  30, SubscriptionType.Free),
                ["Elena Moraru"] = new UserProfile("Elena Moraru",  19, SubscriptionType.Standard),
            };

            // Conținut R-rated (film pentru adulți)
            var filmAbis = new Movie("Abis", "Un actor din Chișinău se luptă cu demonii interiori.",
                Genre.Drama, ContentRating.R, 96, "Vlad Druc");

            // Proxy se interpune — clientul folosește IContentPlayer, nu Movie direct
            IContentPlayer proxy = new ContentAccessProxy(filmAbis, userRegistry);

            Console.WriteLine($"  Conținut protejat: '{proxy.GetTitle()}' (R-rated)");
            Console.WriteLine($"  Info publică (fără autentificare):\n{proxy.GetInfo()}\n");

            // Test acces: utilizatori diferiți
            Console.WriteLine("  Teste de acces:");

            // Ion: 25 ani, Premium → PERMIS
            Console.WriteLine($"\n  → Ion Cojocaru (25 ani, Premium):");
            Console.WriteLine($"    {proxy.Play("Ion Cojocaru")}");

            // Maria: 15 ani, Standard → REFUZAT (vârstă insuficientă)
            Console.WriteLine($"\n  → Maria Rusu (15 ani, Standard):");
            Console.WriteLine($"    {proxy.Play("Maria Rusu")}");

            // Andrei: 30 ani, Free → REFUZAT (abonament insuficient)
            Console.WriteLine($"\n  → Andrei Lupan (30 ani, Free):");
            Console.WriteLine($"    {proxy.Play("Andrei Lupan")}");

            // Elena: 19 ani, Standard → REFUZAT (R-rated cu Standard)
            Console.WriteLine($"\n  → Elena Moraru (19 ani, Standard):");
            Console.WriteLine($"    {proxy.Play("Elena Moraru")}");

            // Utilizator neautentificat
            Console.WriteLine($"\n  → Utilizator necunoscut:");
            Console.WriteLine($"    {proxy.Play("Hacker123")}");

            // Log complet
            var accessProxy = (ContentAccessProxy)proxy;
            Console.WriteLine($"\n{accessProxy.GetAccessReport()}");

            // ── Virtual Proxy: Lazy Loading ───────────────────────────────────────
            Console.WriteLine("  Scenariu B — Virtual Proxy: Lazy loading (încărcare la cerere).\n");

            var movie1 = new Movie("Carbon", "Drama din nordul Moldovei.",
                Genre.Drama, ContentRating.PG13, 102, "Ion Borș");
            var movie2 = new Movie("Hotarul", "Povestea unui sat moldovenesc.",
                Genre.Drama, ContentRating.PG, 88, "Vasile Pascaru");
            var movie3 = new Movie("Lăutarii", "Povestea lăutarului Toma Alimoș.",
                Genre.Drama, ContentRating.PG, 87, "Emil Loteanu");

            // Creăm Virtual Proxies — subiectele reale NU sunt încă create
            IContentPlayer lazy1 = new LazyContentProxy(movie1);
            IContentPlayer lazy2 = new LazyContentProxy(movie2);
            IContentPlayer lazy3 = new LazyContentProxy(movie3);

            Console.WriteLine("  3 Virtual Proxies create — subiectele reale NU sunt încă inițializate.");
            Console.WriteLine("\n  GetInfo() fără inițializare (rapid, fără alocare):");
            Console.WriteLine($"    {lazy1.GetInfo()}\n");

            Console.WriteLine("  Utilizatorul alege să vizioneze doar primul film:");
            Console.WriteLine($"    {lazy1.Play("Ion Cojocaru")}");
            Console.WriteLine($"    lazy2 inițializat? {((LazyContentProxy)lazy2).IsLoaded}");
            Console.WriteLine($"    lazy3 inițializat? {((LazyContentProxy)lazy3).IsLoaded}");

            Console.WriteLine("\n  Al doilea Play pe același film — subiectul e deja încărcat:");
            Console.WriteLine($"    {lazy1.Play("Ion Cojocaru")}");

            Console.WriteLine("\n  Concluzie Proxy:");
            Console.WriteLine("  Protection Proxy: securitate fără a modifica MediaContent.");
            Console.WriteLine("  Virtual Proxy: lazy loading — 200 filme în UI → 0-200 obiecte grele.\n");
        }

        // ============================================================
        // LAB 6 — DEMO STRATEGY PATTERN
        // ============================================================
        static void DemoStrategy()
        {
            Console.WriteLine("┌──────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  LAB 6 — STRATEGY PATTERN: Algoritmi de recomandare          │");
            Console.WriteLine("└──────────────────────────────────────────────────────────────┘");

            Console.WriteLine("\n  Scenariu: Platforma alege dinamic algoritmul de recomandare");
            Console.WriteLine("  fără a modifica logica existentă.\n");

            var allContent = ApiEndpoints.GetAllContent();
            var context = new RecommendationContext(new TopRatedStrategy());

            Console.WriteLine($"  [Strategie 1] {context.CurrentStrategy}:");
            foreach (var c in context.GetRecommendations(allContent, 3))
                Console.WriteLine($"    • {c.Title} — {c.AverageRating}/5");

            context.SetStrategy(new MostViewedStrategy());
            Console.WriteLine($"\n  [Strategie 2] {context.CurrentStrategy}:");
            foreach (var c in context.GetRecommendations(allContent, 3))
                Console.WriteLine($"    • {c.Title} — {c.ViewsCount} vizualizări");

            context.SetStrategy(new ByGenreStrategy(Genre.Drama));
            Console.WriteLine($"\n  [Strategie 3] {context.CurrentStrategy}:");
            foreach (var c in context.GetRecommendations(allContent, 3))
                Console.WriteLine($"    • {c.Title} ({c.Genre})");

            context.SetStrategy(new ShortContentStrategy());
            Console.WriteLine($"\n  [Strategie 4] {context.CurrentStrategy}:");
            foreach (var c in context.GetRecommendations(allContent, 3))
                Console.WriteLine($"    • {c.Title} — {c.GetDuration()} min");

            Console.WriteLine("\n  Concluzie Strategy:");
            Console.WriteLine("  4 algoritmi diferiți, 0 modificări în RecommendationContext.\n");
        }

        // ============================================================
        // LAB 6 — DEMO OBSERVER PATTERN
        // ============================================================
        static void DemoObserver()
        {
            Console.WriteLine("┌──────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  LAB 6 — OBSERVER PATTERN: Notificări automate conținut      │");
            Console.WriteLine("└──────────────────────────────────────────────────────────────┘");

            Console.WriteLine("\n  Scenariu: Când se adaugă conținut nou pe platformă,");
            Console.WriteLine("  toți abonații sunt notificați automat.\n");

            var publisher = new ContentPublisher();
            var userIon   = new UserNotificationObserver("Ion Cojocaru");
            var userMaria = new UserNotificationObserver("Maria Rusu");
            var admin     = new AdminDashboardObserver();
            var recEngine = new RecommendationObserver();

            publisher.Subscribe(userIon);
            publisher.Subscribe(userMaria);
            publisher.Subscribe(admin);
            publisher.Subscribe(recEngine);

            Console.WriteLine($"  Observatori abonați: {publisher.ObserverCount}\n");

            var newMovie = new Movie("Ultima Frontieră", "Un film nou moldovenesc.", Genre.Drama, ContentRating.PG13, 95, "Ion Borș");
            Console.WriteLine("  [Publisher] Adăugăm film nou: 'Ultima Frontieră'");
            publisher.NotifyNewContent(newMovie);

            Console.WriteLine("\n  [Publisher] Maria se dezabonează.");
            publisher.Unsubscribe(userMaria);

            Console.WriteLine($"\n  [Publisher] Eliminăm conținut: 'Hotarul'");
            publisher.NotifyContentRemoved("Hotarul");

            Console.WriteLine($"\n  Admin a înregistrat {admin.AddCount} adăugări și {admin.RemoveCount} eliminări.");
            Console.WriteLine($"  RecommendationEngine indexează {recEngine.IndexedContent.Count} titlu(ri) nou(noi).");
            Console.WriteLine($"  Ion a primit {userIon.Notifications.Count} notificări.");
            Console.WriteLine("\n  Concluzie Observer:");
            Console.WriteLine("  Publisher nu știe câți/care observatori există — cuplaj zero.\n");
        }

        // ============================================================
        // LAB 6 — DEMO COMMAND PATTERN
        // ============================================================
        static void DemoCommand()
        {
            Console.WriteLine("┌──────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  LAB 6 — COMMAND PATTERN: Watchlist cu Undo/Redo             │");
            Console.WriteLine("└──────────────────────────────────────────────────────────────┘");

            Console.WriteLine("\n  Scenariu: Utilizatorul gestionează watchlist-ul cu posibilitate");
            Console.WriteLine("  de Undo/Redo pentru fiecare acțiune.\n");

            var watchlist = new Watchlist("Ion Cojocaru");
            var history   = new CommandHistory();

            history.Execute(new AddToWatchlistCommand(watchlist, "Carbon"));
            history.Execute(new AddToWatchlistCommand(watchlist, "Plaha"));
            history.Execute(new AddToWatchlistCommand(watchlist, "Lăutarii"));
            Console.WriteLine($"  Watchlist după 3 adăugări: [{string.Join(", ", watchlist.Items)}]");

            history.Execute(new RateContentCommand(watchlist, "Carbon", 4.5));
            Console.WriteLine($"  Rating Carbon: {watchlist.Ratings["Carbon"]}/5");

            history.Execute(new RemoveFromWatchlistCommand(watchlist, "Plaha"));
            Console.WriteLine($"  Watchlist după eliminare 'Plaha': [{string.Join(", ", watchlist.Items)}]");

            Console.WriteLine($"\n  [UNDO] → {history.Undo()}");
            Console.WriteLine($"  Watchlist după Undo: [{string.Join(", ", watchlist.Items)}]");

            Console.WriteLine($"\n  [UNDO] → {history.Undo()}");
            Console.WriteLine($"  Rating Carbon după Undo rating: {(watchlist.Ratings.ContainsKey("Carbon") ? watchlist.Ratings["Carbon"].ToString() : "șters")}");

            Console.WriteLine($"\n  [REDO] → {history.Redo()}");
            Console.WriteLine($"  Rating Carbon după Redo: {watchlist.Ratings["Carbon"]}/5");

            Console.WriteLine($"\n  Istoric complet ({history.Log.Count} înregistrări):");
            foreach (var entry in history.Log)
                Console.WriteLine($"    {entry}");

            Console.WriteLine("\n  Concluzie Command:");
            Console.WriteLine("  Fiecare acțiune este un obiect — undo/redo, istoric complet.\n");
        }

        // ============================================================
        // LAB 6 — DEMO MEMENTO PATTERN
        // ============================================================
        static void DemoMemento()
        {
            Console.WriteLine("┌──────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  LAB 6 — MEMENTO PATTERN: Salvare/Restaurare sesiune         │");
            Console.WriteLine("└──────────────────────────────────────────────────────────────┘");

            Console.WriteLine("\n  Scenariu: Utilizatorul navighează și filtrează conținut.");
            Console.WriteLine("  Poate reveni la starea anterioară (Back/Forward în browser).\n");

            var session = new UserSessionState("Ion Cojocaru");
            var history = new SessionHistory();

            // Stare inițială
            history.Save(session.SaveState());
            Console.WriteLine($"  [SAVE 1] {session}");

            // Aplică filtru gen
            session.GenreFilter = "Drama";
            session.SortBy = "Views";
            history.Save(session.SaveState());
            Console.WriteLine($"  [SAVE 2] {session}");

            // Caută
            session.SearchQuery = "Carbon";
            session.CurrentPage = 2;
            history.Save(session.SaveState());
            Console.WriteLine($"  [SAVE 3] {session}");

            // Schimbă sortare
            session.SortBy = "Alfabetic";
            history.Save(session.SaveState());
            Console.WriteLine($"  [SAVE 4] {session}");

            Console.WriteLine($"\n  [UNDO/Back] Revenim la starea anterioară...");
            var prev = history.Undo();
            if (prev != null) session.RestoreState(prev);
            Console.WriteLine($"  {session}");

            Console.WriteLine($"\n  [UNDO/Back] Încă o dată...");
            prev = history.Undo();
            if (prev != null) session.RestoreState(prev);
            Console.WriteLine($"  {session}");

            Console.WriteLine($"\n  [REDO/Forward] Mergem înainte...");
            var next = history.Redo();
            if (next != null) session.RestoreState(next);
            Console.WriteLine($"  {session}");

            Console.WriteLine("\n  Concluzie Memento:");
            Console.WriteLine("  Starea internă salvată fără a expune câmpurile private.\n");
        }

        // ============================================================
        // LAB 6 — DEMO ITERATOR PATTERN
        // ============================================================
        static void DemoIterator()
        {
            Console.WriteLine("┌──────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│  LAB 6 — ITERATOR PATTERN: Navigare colecție conținut        │");
            Console.WriteLine("└──────────────────────────────────────────────────────────────┘");

            Console.WriteLine("\n  Scenariu: Parcurgem colecția de conținut prin iteratori");
            Console.WriteLine("  specializați, fără a cunoaște structura internă.\n");

            var allContent = ApiEndpoints.GetAllContent();
            var collection = new ContentCollection(allContent);

            Console.WriteLine($"  Colecție: {collection.Count} titluri totale.\n");

            Console.WriteLine("  [Iterator 1] Secvențial (primele 4):");
            var seqIt = collection.CreateIterator();
            int count = 0;
            while (seqIt.HasNext() && count++ < 4)
            {
                var item = seqIt.Next();
                Console.WriteLine($"    [{seqIt.CurrentIndex}/{seqIt.TotalCount}] {item.Title} — {item.GetType().Name}");
            }

            Console.WriteLine("\n  [Iterator 2] Gen Drama:");
            var dramaIt = collection.CreateGenreIterator(Genre.Drama);
            Console.WriteLine($"  Găsite: {dramaIt.TotalCount} titluri Drama");
            while (dramaIt.HasNext())
            {
                var item = dramaIt.Next();
                Console.WriteLine($"    • {item.Title} ({item.AverageRating}/5)");
            }

            Console.WriteLine("\n  [Iterator 3] Top 3 după rating:");
            var topIt = collection.CreateTopRatedIterator(3);
            int rank = 1;
            while (topIt.HasNext())
            {
                var item = topIt.Next();
                Console.WriteLine($"    #{rank++} {item.Title} — {item.AverageRating}/5");
            }

            Console.WriteLine("\n  [Reset] Iteratorul Top se resetează și reîncepe:");
            topIt.Reset();
            Console.WriteLine($"  HasNext după Reset: {topIt.HasNext()} | Index: {topIt.CurrentIndex}");

            Console.WriteLine("\n  Concluzie Iterator:");
            Console.WriteLine("  3 moduri de traversare, 0 cunoștințe despre List<MediaContent> intern.\n");
        }
    }
}
