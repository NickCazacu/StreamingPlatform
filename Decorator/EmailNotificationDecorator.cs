using System;

namespace StreamingPlatform.Decorator
{
    /// <summary>
    /// CONCRETE DECORATOR — Adaugă canal Email la orice notificare.
    /// Apelează mai întâi componenta învelită, apoi trimite email-ul.
    /// </summary>
    public class EmailNotificationDecorator : NotificationDecorator
    {
        private readonly string _smtpServer;
        private readonly string _fromAddress;

        public EmailNotificationDecorator(
            IStreamNotification wrapped,
            string smtpServer = "smtp.streamzone.md",
            string fromAddress = "noreply@streamzone.md")
            : base(wrapped)
        {
            _smtpServer  = smtpServer;
            _fromAddress = fromAddress;
        }

        public override void Send(string userName, string message)
        {
            base.Send(userName, message);
            SendEmail(userName, message);
        }

        private void SendEmail(string userName, string message)
        {
            string subject = message.Length > 50 ? message[..47] + "..." : message;
            Console.WriteLine($"      [EMAIL via {_smtpServer}] De la: {_fromAddress} | " +
                              $"Către: {userName}@streamzone.md | Subiect: \"{subject}\"");
        }

        public override string GetChannels() => _wrapped.GetChannels() + " + Email";
    }
}
