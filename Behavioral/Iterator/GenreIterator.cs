using System.Collections.Generic;
using System.Linq;
using StreamingPlatform.Models;

namespace StreamingPlatform.Behavioral.Iterator
{
    public class GenreIterator : IContentIterator
    {
        private readonly List<MediaContent> _items;
        private int _index;

        public int TotalCount => _items.Count;
        public int CurrentIndex => _index;

        public GenreIterator(IEnumerable<MediaContent> allContent, Genre genre)
        {
            _items = allContent.Where(c => c.Genre == genre).ToList();
            _index = 0;
        }

        public bool HasNext() => _index < _items.Count;
        public MediaContent Next() => _items[_index++];
        public void Reset() => _index = 0;
    }
}
