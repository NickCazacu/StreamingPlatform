using System.Collections.Generic;
using System.Linq;
using StreamingPlatform.Models;

namespace StreamingPlatform.Behavioral.Strategy
{
    public class MostViewedStrategy : IRecommendationStrategy
    {
        public string StrategyName => "Most Viewed (Vizualizări descrescător)";

        public List<MediaContent> GetRecommendations(List<MediaContent> allContent, int maxResults = 5)
        {
            return allContent
                .OrderByDescending(c => c.ViewsCount)
                .Take(maxResults)
                .ToList();
        }
    }
}
