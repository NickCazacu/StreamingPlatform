using StreamingPlatform.Models;

namespace StreamingPlatform.Behavioral.Iterator
{
    public interface IContentIterator
    {
        bool HasNext();
        MediaContent Next();
        void Reset();
        int TotalCount { get; }
        int CurrentIndex { get; }
    }
}
