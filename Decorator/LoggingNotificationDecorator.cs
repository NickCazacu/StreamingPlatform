using System;
using System.Collections.Generic;

namespace StreamingPlatform.Decorator
{
    /// <summary>
    /// CONCRETE DECORATOR — Adaugă logging detaliat pentru audit.
    /// Înregistrează toate notificările cu timestamp, canale și status.
    /// Poate fi pus la orice nivel al lanțului.
    /// </summary>
    public class LoggingNotificationDecorator : NotificationDecorator
    {
        private readonly List<string> _auditLog = new();

        public LoggingNotificationDecorator(IStreamNotification wrapped)
            : base(wrapped) { }

        public override void Send(string userName, string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string entry = $"[AUDIT {timestamp}] Canale: {_wrapped.GetChannels()} | " +
                           $"Dest: {userName} | Mesaj: \"{message}\"";
            _auditLog.Add(entry);

            base.Send(userName, message);

            _auditLog[^1] += " | Status: TRIMIS";
            Console.WriteLine($"      [AUDIT LOG] Notificare înregistrată (total: {_auditLog.Count})");
        }

        public override string GetChannels() => _wrapped.GetChannels() + " + Audit";

        public IReadOnlyList<string> GetAuditLog() => _auditLog.AsReadOnly();

        public string GetAuditReport()
        {
            if (_auditLog.Count == 0) return "   Nicio notificare înregistrată.";
            string report = $"   Raport audit ({_auditLog.Count} notificări):\n";
            foreach (var entry in _auditLog)
                report += $"   {entry}\n";
            return report;
        }
    }
}
