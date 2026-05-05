using System;
using System.Collections.Generic;
using StreamingPlatform.Models;

namespace StreamingPlatform.Behavioral.Observer
{
    public class AdminDashboardObserver : IContentObserver
    {
        private int _addCount;
        private int _removeCount;
        private readonly List<string> _auditLog = new();

        public string ObserverName => "AdminDashboard";
        public IReadOnlyList<string> AuditLog => _auditLog.AsReadOnly();
        public int AddCount => _addCount;
        public int RemoveCount => _removeCount;

        public void OnNewContentAdded(MediaContent content)
        {
            _addCount++;
            var msg = $"[ADMIN][{DateTime.Now:HH:mm:ss}] ADĂUGAT #{_addCount}: \"{content.Title}\" | Gen: {content.Genre} | Rating: {content.Rating}";
            _auditLog.Add(msg);
            Console.WriteLine($"  [Observer-Admin] {msg}");
        }

        public void OnContentRemoved(string title)
        {
            _removeCount++;
            var msg = $"[ADMIN][{DateTime.Now:HH:mm:ss}] ELIMINAT #{_removeCount}: \"{title}\"";
            _auditLog.Add(msg);
            Console.WriteLine($"  [Observer-Admin] {msg}");
        }
    }
}
