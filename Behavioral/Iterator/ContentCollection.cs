using System.Collections.Generic;
using System.Linq;
using StreamingPlatform.Models;

namespace StreamingPlatform.Behavioral.Iterator
{
    public class ContentCollection
    {
        private readonly List<MediaContent> _content;

        public ContentCollection(IEnumerable<MediaContent> content)
        {
            _content = content.ToList();
        }

        public int Count => _content.Count;

        public IContentIterator CreateIterator() =>
            new SequentialIterator(_content);

        public IContentIterator CreateGenreIterator(Genre genre) =>
            new GenreIterator(_content, genre);

        public IContentIterator CreateTopRatedIterator(int top = 5) =>
            new TopRatedIterator(_content, top);
    }
}
