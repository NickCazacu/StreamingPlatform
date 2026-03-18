using System;
using System.Collections.Generic;
using StreamingPlatform.Interfaces;

namespace StreamingPlatform.Adapters
{
    // ============================================================
    // ADAPTER PATTERN
    // ============================================================
    // Problema: Avem 3 servicii externe de rating (IMDB, RottenTomatoes,
    // Metacritic). Fiecare are API diferit — metode cu nume diferite,
    // formate de date diferite, scale diferite.
    //
    // Soluția: Creăm un Adapter pentru fiecare serviciu care
    // transformă API-ul specific în interfața comună IExternalRatingService.
    //
    // Clientul lucrează DOAR cu IExternalRatingService — nu știe
    // dacă în spate e IMDB, RottenTomatoes sau Metacritic.
    // ============================================================

    // ============================================================
    // SERVICIU EXTERN 1: IMDB (API incompatibil)
    // Scală: 1-10, metode: FetchImdbScore(), FetchImdbReview()
    // ============================================================
    public class ImdbService
    {
        private readonly Dictionary<string, double> _scores = new Dictionary<string, double>
        {
            { "Reservoir Dogs", 8.3 },
            { "The Dark Knight", 9.0 },
            { "Inception", 8.8 },
            { "Pulp Fiction", 8.9 },
            { "Breaking Bad", 9.5 },
            { "House M.D", 8.7 },
            { "Planet Earth", 9.4 }
        };

        private readonly Dictionary<string, string> _reviews = new Dictionary<string, string>
        {
            { "Reservoir Dogs", "A masterclass in tension and dialogue by Tarantino" },
            { "The Dark Knight", "Heath Ledger delivers an iconic Joker performance" },
            { "Inception", "A visually stunning mind-bending experience" },
            { "Pulp Fiction", "Tarantino's magnum opus of interconnected crime stories" },
            { "Breaking Bad", "The greatest TV drama ever made" },
            { "House M.D", "Hugh Laurie shines as the brilliant diagnostician" },
            { "Planet Earth", "A breathtaking journey through the natural world" }
        };

        // API IMDB — metode specifice, scală 1-10
        public double FetchImdbScore(string title)
        {
            return _scores.ContainsKey(title) ? _scores[title] : 0.0;
        }

        public string FetchImdbReview(string title)
        {
            return _reviews.ContainsKey(title) ? _reviews[title] : "No IMDB review available";
        }

        public bool CheckImdbConnection()
        {
            return true; // Simulăm conexiune activă
        }
    }

    // ============================================================
    // SERVICIU EXTERN 2: ROTTEN TOMATOES (API incompatibil)
    // Scală: 0-100%, metode: GetTomatoMeter(), GetCriticsConsensus()
    // ============================================================
    public class RottenTomatoesService
    {
        private readonly Dictionary<string, int> _tomatoMeter = new Dictionary<string, int>
        {
            { "Reservoir Dogs", 90 },
            { "The Dark Knight", 94 },
            { "Inception", 87 },
            { "Pulp Fiction", 92 },
            { "Breaking Bad", 96 },
            { "House M.D", 85 },
            { "Planet Earth", 98 }
        };

        private readonly Dictionary<string, string> _consensus = new Dictionary<string, string>
        {
            { "Reservoir Dogs", "Gritty, stylish, and electrifyingly acted" },
            { "The Dark Knight", "Dark, complex, and unforgettable" },
            { "Inception", "Smart, innovative, and thrilling" },
            { "Pulp Fiction", "Wickedly funny and stylistically bold" },
            { "Breaking Bad", "Tightly paced and brilliantly acted" },
            { "House M.D", "Witty, addictive medical drama" },
            { "Planet Earth", "Visually stunning nature documentary" }
        };

        // API Rotten Tomatoes — metode specifice, scală 0-100%
        public int GetTomatoMeter(string movieTitle)
        {
            return _tomatoMeter.ContainsKey(movieTitle) ? _tomatoMeter[movieTitle] : 0;
        }

        public string GetCriticsConsensus(string movieTitle)
        {
            return _consensus.ContainsKey(movieTitle) ? _consensus[movieTitle] : "No critics consensus";
        }

        public bool Ping()
        {
            return true;
        }
    }

    // ============================================================
    // SERVICIU EXTERN 3: METACRITIC (API incompatibil)
    // Scală: 0-100 (metascore), metode: RetrieveMetascore(), RetrieveSummary()
    // ============================================================
    public class MetacriticService
    {
        private readonly Dictionary<string, int> _metascores = new Dictionary<string, int>
        {
            { "Reservoir Dogs", 79 },
            { "The Dark Knight", 84 },
            { "Inception", 74 },
            { "Pulp Fiction", 94 },
            { "Breaking Bad", 87 },
            { "House M.D", 73 },
            { "Planet Earth", 91 }
        };

        private readonly Dictionary<string, string> _summaries = new Dictionary<string, string>
        {
            { "Reservoir Dogs", "Tarantino's explosive debut remains a landmark" },
            { "The Dark Knight", "A superhero film elevated to art" },
            { "Inception", "Nolan crafts a complex but rewarding puzzle" },
            { "Pulp Fiction", "A cultural touchstone of modern cinema" },
            { "Breaking Bad", "Television at its absolute finest" },
            { "House M.D", "A fresh take on the medical drama genre" },
            { "Planet Earth", "Nature documentary perfection" }
        };

        // API Metacritic — metode specifice, scală 0-100
        public int RetrieveMetascore(string title)
        {
            return _metascores.ContainsKey(title) ? _metascores[title] : 0;
        }

        public string RetrieveSummary(string title)
        {
            return _summaries.ContainsKey(title) ? _summaries[title] : "No Metacritic summary";
        }

        public bool IsServiceOnline()
        {
            return true;
        }
    }

    // ============================================================
    // ADAPTER 1: IMDB → IExternalRatingService
    // Convertește scală 1-10 → 1-10 (păstrează), adaptează numele metodelor
    // ============================================================
    public class ImdbAdapter : IExternalRatingService
    {
        private readonly ImdbService _imdbService;

        public ImdbAdapter(ImdbService imdbService)
        {
            _imdbService = imdbService;
        }

        public string GetServiceName() => "IMDB";

        public double GetRating(string contentTitle)
        {
            // IMDB: scală 1-10, interfața noastră: scală 1-10 → fără conversie
            return _imdbService.FetchImdbScore(contentTitle);
        }

        public string GetReview(string contentTitle)
        {
            return _imdbService.FetchImdbReview(contentTitle);
        }

        public bool IsAvailable()
        {
            return _imdbService.CheckImdbConnection();
        }
    }

    // ============================================================
    // ADAPTER 2: ROTTEN TOMATOES → IExternalRatingService
    // Convertește scală 0-100% → 1-10, adaptează numele metodelor
    // ============================================================
    public class RottenTomatoesAdapter : IExternalRatingService
    {
        private readonly RottenTomatoesService _rtService;

        public RottenTomatoesAdapter(RottenTomatoesService rtService)
        {
            _rtService = rtService;
        }

        public string GetServiceName() => "Rotten Tomatoes";

        public double GetRating(string contentTitle)
        {
            // RT: scală 0-100%, interfața noastră: 1-10
            // Conversie: 94% → 9.4
            int tomatoMeter = _rtService.GetTomatoMeter(contentTitle);
            return Math.Round(tomatoMeter / 10.0, 1);
        }

        public string GetReview(string contentTitle)
        {
            return _rtService.GetCriticsConsensus(contentTitle);
        }

        public bool IsAvailable()
        {
            return _rtService.Ping();
        }
    }

    // ============================================================
    // ADAPTER 3: METACRITIC → IExternalRatingService
    // Convertește scală 0-100 → 1-10, adaptează numele metodelor
    // ============================================================
    public class MetacriticAdapter : IExternalRatingService
    {
        private readonly MetacriticService _metacriticService;

        public MetacriticAdapter(MetacriticService metacriticService)
        {
            _metacriticService = metacriticService;
        }

        public string GetServiceName() => "Metacritic";

        public double GetRating(string contentTitle)
        {
            // Metacritic: scală 0-100, interfața noastră: 1-10
            // Conversie: 84 → 8.4
            int metascore = _metacriticService.RetrieveMetascore(contentTitle);
            return Math.Round(metascore / 10.0, 1);
        }

        public string GetReview(string contentTitle)
        {
            return _metacriticService.RetrieveSummary(contentTitle);
        }

        public bool IsAvailable()
        {
            return _metacriticService.IsServiceOnline();
        }
    }

    // ============================================================
    // AGGREGATOR — folosește toate adaptoarele uniform
    // Demonstrează că clientul lucrează doar cu IExternalRatingService
    // ============================================================
    public class RatingAggregator
    {
        private readonly List<IExternalRatingService> _services = new List<IExternalRatingService>();

        public void AddService(IExternalRatingService service)
        {
            _services.Add(service);
        }

        /// <summary>
        /// Calculează media rating-urilor de la toate serviciile.
        /// Nu știe dacă e IMDB, RT sau Metacritic — lucrează doar cu interfața.
        /// </summary>
        public double GetAverageRating(string contentTitle)
        {
            if (_services.Count == 0) return 0;

            double total = 0;
            int available = 0;

            foreach (var service in _services)
            {
                if (service.IsAvailable())
                {
                    double rating = service.GetRating(contentTitle);
                    if (rating > 0)
                    {
                        total += rating;
                        available++;
                    }
                }
            }

            return available > 0 ? Math.Round(total / available, 1) : 0;
        }

        /// <summary>
        /// Returnează toate review-urile de la toate serviciile.
        /// </summary>
        public string GetAllReviews(string contentTitle)
        {
            var result = "";
            foreach (var service in _services)
            {
                if (service.IsAvailable())
                {
                    result += $"  [{service.GetServiceName()}] {service.GetReview(contentTitle)}\n";
                }
            }
            return result;
        }

        /// <summary>
        /// Returnează rating-urile individuale de la fiecare serviciu.
        /// </summary>
        public string GetDetailedRatings(string contentTitle)
        {
            var result = "";
            foreach (var service in _services)
            {
                if (service.IsAvailable())
                {
                    result += $"  {service.GetServiceName()}: {service.GetRating(contentTitle)}/10\n";
                }
            }
            return result;
        }
    }
}