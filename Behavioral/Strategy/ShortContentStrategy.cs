using System.Collections.Generic;
using System.Linq;
using StreamingPlatform.Models;

namespace StreamingPlatform.Behavioral.Strategy
{
    public class ShortContentStrategy : IRecommendationStrategy
    {
        public string StrategyName => "Short Content (Durată crescătoare)";

        public List<MediaContent> GetRecommendations(List<MediaContent> allContent, int maxResults = 5)
        {
            return allContent
                .OrderBy(c => c.GetDuration())
                .Take(maxResults)
                .ToList();
        }
    }
}
