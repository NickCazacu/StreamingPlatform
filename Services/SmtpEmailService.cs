using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StreamingPlatform.Services
{
    /// <summary>
    /// Trimite email-uri reale prin SMTP (Gmail/Outlook/SendGrid etc.).
    /// Configurabil din appsettings.json secțiunea "Email:Smtp".
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly bool _enableSsl;
        private readonly string _username;
        private readonly string _password;
        private readonly string _fromAddress;
        private readonly string _fromName;
        private readonly ILogger<SmtpEmailService>? _logger;

        public bool IsRealProvider => !string.IsNullOrWhiteSpace(_username) && !string.IsNullOrWhiteSpace(_password);
        public string ProviderName => $"SMTP ({_host}:{_port})";

        public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService>? logger = null)
        {
            var section = config.GetSection("Email:Smtp");
            _host        = section["Host"]        ?? "smtp.gmail.com";
            _port        = int.TryParse(section["Port"], out var p) ? p : 587;
            _enableSsl   = !bool.TryParse(section["EnableSsl"], out var ssl) || ssl;
            _username    = section["Username"]    ?? "";
            _password    = section["AppPassword"] ?? "";
            _fromAddress = section["FromAddress"] ?? _username;
            _fromName    = section["FromName"]    ?? "StreamZone";
            _logger = logger;
        }

        public async Task<bool> SendAsync(string toAddress, string subject, string htmlBody)
        {
            if (!IsRealProvider)
            {
                _logger?.LogWarning("SMTP nu e configurat — email NU a fost trimis la {To}", toAddress);
                return false;
            }

            try
            {
                using var msg = new MailMessage
                {
                    From = new MailAddress(_fromAddress, _fromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                msg.To.Add(toAddress);

                using var client = new SmtpClient(_host, _port)
                {
                    Credentials = new NetworkCredential(_username, _password),
                    EnableSsl = _enableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                await client.SendMailAsync(msg);
                _logger?.LogInformation("Email trimis cu succes la {To} — {Subject}", toAddress, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Eroare la trimitere email către {To}: {Message}", toAddress, ex.Message);
                Console.WriteLine($"  ⚠ [Email] Eroare la {toAddress}: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>Fallback când SMTP nu e configurat — log la consolă.</summary>
    public class MockEmailService : IEmailService
    {
        public bool IsRealProvider => false;
        public string ProviderName => "Mock (consolă)";

        public Task<bool> SendAsync(string toAddress, string subject, string htmlBody)
        {
            Console.WriteLine($"  [Email-Mock] → {toAddress} | Subiect: \"{subject}\"");
            return Task.FromResult(true);
        }
    }
}
