using System;

namespace StreamingPlatform.Decorator
{
    /// <summary>
    /// CONCRETE DECORATOR — Adaugă canal SMS la orice notificare.
    /// SMS are limitare de 160 caractere — mesajele lungi sunt trunchiate.
    /// </summary>
    public class SmsNotificationDecorator : NotificationDecorator
    {
        private readonly string _gateway;
        private const int SmsMaxLength = 160;

        public SmsNotificationDecorator(
            IStreamNotification wrapped,
            string gateway = "Orange Moldova")
            : base(wrapped)
        {
            _gateway = gateway;
        }

        public override void Send(string userName, string message)
        {
            base.Send(userName, message);
            SendSms(userName, message);
        }

        private void SendSms(string userName, string message)
        {
            string smsText = message.Length > SmsMaxLength
                ? message[..(SmsMaxLength - 3)] + "..."
                : message;

            int phoneHash = Math.Abs(userName.GetHashCode()) % 9000 + 1000;
            Console.WriteLine($"      [SMS via {_gateway}] → +373-{phoneHash}: \"{smsText}\" ({smsText.Length} ch)");
        }

        public override string GetChannels() => _wrapped.GetChannels() + " + SMS";
    }
}
