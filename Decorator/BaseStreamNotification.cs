using System;
using System.Collections.Generic;

namespace StreamingPlatform.Decorator
{
    /// <summary>
    /// CONCRETE COMPONENT — Notificare internă prin consolă/log.
    /// Componenta minimă — celelalte funcționalități se adaugă prin decoratoare.
    /// </summary>
    public class BaseStreamNotification : IStreamNotification
    {
        private readonly List<string> _log = new();

        public void Send(string userName, string message)
        {
            string entry = $"[{DateTime.Now:HH:mm:ss}] {userName}: {message}";
            _log.Add(entry);
            Console.WriteLine($"      [Notificare] → {userName}: {message}");
        }

        public string GetChannels() => "Consolă";

        public IReadOnlyList<string> GetLog() => _log.AsReadOnly();
    }
}
