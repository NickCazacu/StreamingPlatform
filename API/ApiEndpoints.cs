using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using StreamingPlatform.Models;
using StreamingPlatform.Builders;
using StreamingPlatform.Adapters;
using StreamingPlatform.Services;

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

        /// <summary>
        /// Încarcă datele inițiale (filmele, serialele, documentarele din proiect).
        /// </summary>
        public static void LoadData()
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

            _movies = new List<Movie> { movie1, movie2, movie3, movie4, movie5 };

            // Adăugăm rating-uri
            movie1.AddRating(4.2); movie1.AddRating(4.5); movie1.AddRating(4.0);
            movie2.AddRating(4.6); movie2.AddRating(5.0); movie2.AddRating(4.8);
            movie3.AddRating(3.9); movie3.AddRating(4.1); movie3.AddRating(4.3);
            movie4.AddRating(4.0); movie4.AddRating(4.2); movie4.AddRating(4.5);
            movie5.AddRating(4.8); movie5.AddRating(5.0); movie5.AddRating(4.9);

            // ── Seriale moldovenești ────────────────────────────────────────────
            var series1 = new SeriesBuilder()
                .SetTitle("Plaha")
                .SetDescription("O dramă despre crima organizată, traficul de droguri și oameni prinși între loialitate și supraviețuire. Inspirat din realitățile spațiului post-sovietic.")
                .SetGenre(Genre.Drama).SetRating(ContentRating.R)
                .SetCreator("Iuri Moroz").SetSeasons(1).SetEpisodes(8).SetEpisodeDuration(52)
                .MarkAsCompleted().Build();

            _series = new List<Series> { series1 };
            series1.AddRating(4.7); series1.AddRating(4.9); series1.AddRating(5.0);

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

            _documentaries = new List<Documentary> { doc1, doc2 };
            doc1.AddRating(4.6); doc1.AddRating(4.8); doc1.AddRating(5.0);
            doc2.AddRating(4.4); doc2.AddRating(4.7); doc2.AddRating(4.5);

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

            // --- CĂUTARE ---
            app.MapGet("/api/search", (string? q, string? genre) =>
            {
                var results = new List<object>();

                var movies = _movies.AsEnumerable();
                var series = _series.AsEnumerable();
                var docs = _documentaries.AsEnumerable();

                if (!string.IsNullOrEmpty(q))
                {
                    movies = movies.Where(m => m.Title.Contains(q, StringComparison.OrdinalIgnoreCase));
                    series = series.Where(s => s.Title.Contains(q, StringComparison.OrdinalIgnoreCase));
                    docs = docs.Where(d => d.Title.Contains(q, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(genre))
                {
                    if (Enum.TryParse<Genre>(genre, true, out var g))
                    {
                        movies = movies.Where(m => m.Genre == g);
                        series = series.Where(s => s.Genre == g);
                        docs = docs.Where(d => d.Genre == g);
                    }
                }

                results.AddRange(movies.Select(m => new { m.Id, m.Title, m.Description, Genre = m.Genre.ToString(), Type = "Movie" }));
                results.AddRange(series.Select(s => new { s.Id, s.Title, s.Description, Genre = s.Genre.ToString(), Type = "Series" }));
                results.AddRange(docs.Select(d => new { d.Id, d.Title, d.Description, Genre = d.Genre.ToString(), Type = "Documentary" }));

                return results;
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
        }
    }
}
