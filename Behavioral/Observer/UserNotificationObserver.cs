using System;
using System.Collections.Generic;
using StreamingPlatform.Models;

namespace StreamingPlatform.Behavioral.Observer
{
    public class UserNotificationObserver : IContentObserver
    {
        private readonly string _userName;
        private readonly List<string> _notifications = new();

        public string ObserverName => $"User:{_userName}";
        public IReadOnlyList<string> Notifications => _notifications.AsReadOnly();

        public UserNotificationObserver(string userName)
        {
            _userName = userName;
        }

        public void OnNewContentAdded(MediaContent content)
        {
            var msg = $"[{DateTime.Now:HH:mm:ss}] {_userName}: Film nou — \"{content.Title}\" ({content.Genre}, {content.Rating})";
            _notifications.Add(msg);
            Console.WriteLine($"  [Observer-User] {msg}");
        }

        public void OnContentRemoved(string title)
        {
            var msg = $"[{DateTime.Now:HH:mm:ss}] {_userName}: Conținut eliminat — \"{title}\"";
            _notifications.Add(msg);
            Console.WriteLine($"  [Observer-User] {msg}");
        }
    }
}
