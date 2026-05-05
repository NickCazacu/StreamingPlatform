using System.Collections.Generic;
using System.Linq;
using StreamingPlatform.Models;

namespace StreamingPlatform.Behavioral.Strategy
{
    public class ByGenreStrategy : IRecommendationStrategy
    {
        private readonly Genre _genre;

        public string StrategyName => $"By Genre ({_genre})";

        public ByGenreStrategy(Genre genre)
        {
            _genre = genre;
        }

        public List<MediaContent> GetRecommendations(List<MediaContent> allContent, int maxResults = 5)
        {
            return allContent
                .Where(c => c.Genre == _genre)
                .OrderByDescending(c => c.AverageRating)
                .Take(maxResults)
                .ToList();
        }
    }
}
