using System.Collections.Generic;
using System.Linq;
using StreamingPlatform.Models;

namespace StreamingPlatform.Behavioral.Strategy
{
    public class TopRatedStrategy : IRecommendationStrategy
    {
        public string StrategyName => "Top Rated (Rating descrescător)";

        public List<MediaContent> GetRecommendations(List<MediaContent> allContent, int maxResults = 5)
        {
            return allContent
                .OrderByDescending(c => c.AverageRating)
                .Take(maxResults)
                .ToList();
        }
    }
}
