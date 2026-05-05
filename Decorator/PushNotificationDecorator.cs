using System;

namespace StreamingPlatform.Decorator
{
    /// <summary>
    /// CONCRETE DECORATOR — Adaugă notificări push în aplicația mobilă.
    /// Adaugă pictograme contextuale în funcție de conținutul mesajului.
    /// </summary>
    public class PushNotificationDecorator : NotificationDecorator
    {
        private readonly string _appId;
        private readonly string _platform;

        public PushNotificationDecorator(
            IStreamNotification wrapped,
            string appId = "StreamZone",
            string platform = "FCM")
            : base(wrapped)
        {
            _appId    = appId;
            _platform = platform;
        }

        public override void Send(string userName, string message)
        {
            base.Send(userName, message);
            SendPush(userName, message);
        }

        private void SendPush(string userName, string message)
        {
            string icon = GetContextIcon(message);
            string priority = message.Contains("expiră", StringComparison.OrdinalIgnoreCase) ||
                              message.Contains("urgent", StringComparison.OrdinalIgnoreCase)
                              ? "HIGH" : "NORMAL";

            Console.WriteLine($"      [PUSH via {_platform}/{_appId}] → {userName} | " +
                              $"{icon} \"{message}\" | Prioritate: {priority}");
        }

        private static string GetContextIcon(string message)
        {
            if (message.Contains("abonament", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("expiră",    StringComparison.OrdinalIgnoreCase))  return "⚠";
            if (message.Contains("nou",       StringComparison.OrdinalIgnoreCase) ||
                message.Contains("adăugat",   StringComparison.OrdinalIgnoreCase)) return "🆕";
            if (message.Contains("vizionezi", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("stream",    StringComparison.OrdinalIgnoreCase))  return "▶";
            if (message.Contains("ofertă",    StringComparison.OrdinalIgnoreCase) ||
                message.Contains("reducere",  StringComparison.OrdinalIgnoreCase)) return "🏷";
            return "🔔";
        }

        public override string GetChannels() => _wrapped.GetChannels() + " + Push";
    }
}
