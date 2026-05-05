using System;
using System.Collections.Generic;
using StreamingPlatform.Models;

namespace StreamingPlatform.Behavioral.Observer
{
    public class ContentPublisher
    {
        private readonly List<IContentObserver> _observers = new();
        private readonly List<string> _eventLog = new();

        public IReadOnlyList<string> EventLog => _eventLog.AsReadOnly();
        public int ObserverCount => _observers.Count;

        public void Subscribe(IContentObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void Unsubscribe(IContentObserver observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyNewContent(MediaContent content)
        {
            _eventLog.Add($"[{DateTime.Now:HH:mm:ss}] Conținut nou: '{content.Title}' ({content.GetType().Name})");
            foreach (var obs in _observers)
                obs.OnNewContentAdded(content);
        }

        public void NotifyContentRemoved(string title)
        {
            _eventLog.Add($"[{DateTime.Now:HH:mm:ss}] Conținut eliminat: '{title}'");
            foreach (var obs in _observers)
                obs.OnContentRemoved(title);
        }
    }
}
