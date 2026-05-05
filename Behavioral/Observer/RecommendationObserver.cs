using System;
using System.Collections.Generic;
using StreamingPlatform.Models;

namespace StreamingPlatform.Behavioral.Observer
{
    public class RecommendationObserver : IContentObserver
    {
        private readonly List<MediaContent> _indexedContent = new();

        public string ObserverName => "RecommendationEngine";
        public IReadOnlyList<MediaContent> IndexedContent => _indexedContent.AsReadOnly();

        public void OnNewContentAdded(MediaContent content)
        {
            _indexedContent.Add(content);
            Console.WriteLine($"  [Observer-Recom] [{DateTime.Now:HH:mm:ss}] Indexat pentru recomandări: \"{content.Title}\"");
        }

        public void OnContentRemoved(string title)
        {
            _indexedContent.RemoveAll(c => c.Title == title);
            Console.WriteLine($"  [Observer-Recom] [{DateTime.Now:HH:mm:ss}] Dezindexat: \"{title}\"");
        }
    }
}
