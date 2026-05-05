using StreamingPlatform.Models;

namespace StreamingPlatform.Proxy
{
    /// <summary>
    /// Profilul unui utilizator înregistrat în sistem.
    /// Folosit de ContentAccessProxy pentru verificarea drepturilor de acces.
    /// </summary>
    public record UserProfile(string Name, int Age, SubscriptionType Subscription);
}
