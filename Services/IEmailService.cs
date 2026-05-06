using System.Threading.Tasks;

namespace StreamingPlatform.Services
{
    public interface IEmailService
    {
        /// <summary>Trimite un email HTML. Returnează true la succes, false la eroare.</summary>
        Task<bool> SendAsync(string toAddress, string subject, string htmlBody);

        /// <summary>True dacă e configurat real (SMTP cu credențiale), false dacă e mock.</summary>
        bool IsRealProvider { get; }

        /// <summary>Numele provider-ului pentru log/debug.</summary>
        string ProviderName { get; }
    }
}
