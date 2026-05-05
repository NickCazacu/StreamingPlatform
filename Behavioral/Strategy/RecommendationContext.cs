using System.Collections.Generic;
using StreamingPlatform.Models;

namespace StreamingPlatform.Behavioral.Strategy
{
    public class RecommendationContext
    {
        private IRecommendationStrategy _strategy;

        public string CurrentStrategy => _strategy.StrategyName;

        public RecommendationContext(IRecommendationStrategy strategy)
        {
            _strategy = strategy;
        }

        public void SetStrategy(IRecommendationStrategy strategy)
        {
            _strategy = strategy;
        }

        public List<MediaContent> GetRecommendations(List<MediaContent> allContent, int maxResults = 5)
        {
            return _strategy.GetRecommendations(allContent, maxResults);
        }
    }
}
