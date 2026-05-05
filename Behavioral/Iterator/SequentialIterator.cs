using System.Collections.Generic;
using StreamingPlatform.Models;

namespace StreamingPlatform.Behavioral.Iterator
{
    public class SequentialIterator : IContentIterator
    {
        private readonly List<MediaContent> _items;
        private int _index;

        public int TotalCount => _items.Count;
        public int CurrentIndex => _index;

        public SequentialIterator(List<MediaContent> items)
        {
            _items = items;
            _index = 0;
        }

        public bool HasNext() => _index < _items.Count;
        public MediaContent Next() => _items[_index++];
        public void Reset() => _index = 0;
    }
}
