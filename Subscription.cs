using System;

namespace StreamingPlatform.Models
{
    public class Subscription
    {
        public SubscriptionType Type { get; private set; }
        public DateTime? ExpiryDate { get; private set; }
        public bool IsActive => ExpiryDate.HasValue && ExpiryDate.Value > DateTime.Now;

        public Subscription()
        {
            Type = SubscriptionType.Free;
            ExpiryDate = null;
        }

        public void Upgrade(SubscriptionType newType, int months)
        {
            Type = newType;
            ExpiryDate = DateTime.Now.AddMonths(months);
        }

        public string GetInfo()
        {
            if (Type == SubscriptionType.Free)
                return "Gratuit (Fără expirare)";
            
            string status = IsActive ? "Activ" : "Expirat";
            return $"{Type} ({status}, Expiră: {ExpiryDate:yyyy-MM-dd})";
        }
    }
}
