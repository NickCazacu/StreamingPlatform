using StreamingPlatform.Models;

namespace StreamingPlatform.Behavioral.Observer
{
    public interface IContentObserver
    {
        string ObserverName { get; }
        void OnNewContentAdded(MediaContent content);
        void OnContentRemoved(string title);
    }
}
