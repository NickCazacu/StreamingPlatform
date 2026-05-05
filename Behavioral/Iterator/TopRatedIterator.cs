using System.Collections.Generic;
using System.Linq;
using StreamingPlatform.Models;

namespace StreamingPlatform.Behavioral.Iterator
{
    public class TopRatedIterator : IContentIterator
    {
        private readonly List<MediaContent> _items;
        private int _index;

        public int TotalCount => _items.Count;
        public int CurrentIndex => _index;

        public TopRatedIterator(IEnumerable<MediaContent> allContent, int top = 5)
        {
            _items = allContent
                .OrderByDescending(c => c.AverageRating)
                .Take(top)
                .ToList();
            _index = 0;
        }

        public bool HasNext() => _index < _items.Count;
        public MediaContent Next() => _items[_index++];
        public void Reset() => _index = 0;
    }
}
