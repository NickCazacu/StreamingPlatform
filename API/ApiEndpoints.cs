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
            // Filme
            var movie1 = new Movie("Reservoir Dogs", "You've been brave enough. For one day",
                Genre.Action, ContentRating.R, 99, "Quentin Tarantino");
            movie1.AddCastMember("Tim Roth");
            movie1.AddCastMember("Michael Madsen");

            var movie2 = new Movie("The Dark Knight", "Batman faces the Joker",
                Genre.Action, ContentRating.PG13, 152, "Christopher Nolan");
            movie2.AddCastMember("Christian Bale");
            movie2.AddCastMember("Heath Ledger");

            var movie3 = new MovieBuilder()
                .SetTitle("Inception")
                .SetDescription("A mind-bending thriller about dream infiltration")
                .SetGenre(Genre.SciFi).SetRating(ContentRating.PG13)
                .SetDuration(148).SetDirector("Christopher Nolan")
                .AddCastMembers("Leonardo DiCaprio", "Tom Hardy", "Ellen Page")
                .Build();

            var movie4 = new MovieBuilder()
                .SetTitle("Pulp Fiction")
                .SetDescription("Interconnected stories of crime in Los Angeles")
                .SetGenre(Genre.Action).SetRating(ContentRating.R)
                .SetDuration(154).SetDirector("Quentin Tarantino")
                .AddCastMembers("John Travolta", "Samuel L. Jackson", "Uma Thurman")
                .Build();

            var movie5 = new MovieBuilder()
                .SetTitle("Interstellar")
                .SetDescription("A team of explorers travel through a wormhole in space")
                .SetGenre(Genre.SciFi).SetRating(ContentRating.PG13)
                .SetDuration(169).SetDirector("Christopher Nolan")
                .AddCastMembers("Matthew McConaughey", "Anne Hathaway")
                .Build();

            _movies = new List<Movie> { movie1, movie2, movie3, movie4, movie5 };

            // Adăugăm rating-uri
            foreach (var m in _movies) { m.AddRating(4); m.AddRating(4.5); m.AddRating(5); }

            // Seriale
            var series1 = new SeriesBuilder()
                .SetTitle("Breaking Bad")
                .SetDescription("A chemistry teacher turns to crime")
                .SetGenre(Genre.Drama).SetRating(ContentRating.R)
                .SetCreator("Vince Gilligan").SetSeasons(5).SetEpisodes(62).SetEpisodeDuration(47)
                .MarkAsCompleted().Build();

            var series2 = new SeriesBuilder()
                .SetTitle("House M.D")
                .SetDescription("A brilliant but unconventional doctor solves medical mysteries")
                .SetGenre(Genre.Drama).SetRating(ContentRating.R)
                .SetCreator("David Shore").SetSeasons(8).SetEpisodes(177).SetEpisodeDuration(50)
                .MarkAsCompleted().Build();

            var series3 = new SeriesBuilder()
                .SetTitle("Stranger Things")
                .SetDescription("A group of kids face supernatural forces in a small town")
                .SetGenre(Genre.SciFi).SetRating(ContentRating.PG13)
                .SetCreator("The Duffer Brothers").SetSeasons(4).SetEpisodes(34).SetEpisodeDuration(55)
                .MarkAsOngoing().Build();

            var series4 = new SeriesBuilder()
                .SetTitle("The Witcher")
                .SetDescription("Geralt of Rivia, a monster hunter, navigates a world of magic")
                .SetGenre(Genre.Fantasy).SetRating(ContentRating.R)
                .SetCreator("Lauren Schmidt").SetSeasons(3).SetEpisodes(24).SetEpisodeDuration(60)
                .MarkAsOngoing().Build();

            _series = new List<Series> { series1, series2, series3, series4 };
            foreach (var s in _series) { s.AddRating(4.5); s.AddRating(5); }

            // Documentare
            var doc1 = new DocumentaryBuilder()
                .SetTitle("Planet Earth")
                .SetDescription("Exploration of the natural world and its diverse ecosystems")
                .SetRating(ContentRating.G).SetDuration(60)
                .SetTopic("Nature").SetNarrator("David Attenborough")
                .MarkAsEducational().Build();

            var doc2 = new DocumentaryBuilder()
                .SetTitle("Cosmos")
                .SetDescription("A journey through the universe and the story of space exploration")
                .SetRating(ContentRating.G).SetDuration(55)
                .SetTopic("Space").SetNarrator("Neil deGrasse Tyson")
                .MarkAsEducational().Build();

            _documentaries = new List<Documentary> { doc1, doc2 };
            foreach (var d in _documentaries) { d.AddRating(4.8); d.AddRating(5); }

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
