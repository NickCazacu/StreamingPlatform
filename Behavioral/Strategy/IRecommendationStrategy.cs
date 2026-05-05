using System.Collections.Generic;
using StreamingPlatform.Models;

namespace StreamingPlatform.Behavioral.Strategy
{
    public interface IRecommendationStrategy
    {
        string StrategyName { get; }
        List<MediaContent> GetRecommendations(List<MediaContent> allContent, int maxResults = 5);
    }
}
