using System;
using StreamingPlatform.Services;

namespace StreamingPlatform.Flyweight
{
    /// <summary>
    /// CONTEXT — Sesiunea individuală de streaming per utilizator.
    /// Conține starea extrinsecă (unică per sesiune) și o REFERINȚĂ
    /// la obiectul flyweight partajat (nu o copie proprie!).
    ///
    /// 1.000.000 sesiuni = 1.000.000 obiecte StreamingSession,
    /// dar TOȚI referă aceleași ~6 obiecte StreamQuality din pool.
    /// </summary>
    public class StreamingSession
    {
        // ── Stare EXTRINSECĂ (unică per sesiune) ─────────────────────────
        public string UserName     { get; }
        public string ContentTitle { get; }
        public string DeviceType   { get; }
        public DateTime StartedAt  { get; }

        // Referință la flyweight — NU o copie proprie!
        private readonly StreamQuality _quality;

        public StreamingSession(string userName, string contentTitle,
                                string deviceType, string qualityLevel)
        {
            UserName     = userName;
            ContentTitle = contentTitle;
            DeviceType   = deviceType;
            StartedAt    = DateTime.Now;

            // Obținem flyweight-ul din fabrică — posibil același obiect ca alte sesiuni
            _quality = StreamQualityFactory.GetQuality(qualityLevel);
        }

        /// <summary>
        /// Pornește redarea — transmite starea extrinsecă la flyweight.
        /// Flyweight-ul nu stochează aceste date, le folosește doar local.
        /// </summary>
        public string Play()
        {
            string result = _quality.Render(UserName, ContentTitle, DeviceType);
            PlatformManager.Instance.IncrementStreams();
            return result;
        }

        public string GetQualityInfo()  => _quality.GetDescription();
        public string GetQualityLevel() => _quality.Resolution;

        /// <summary>
        /// Verifică dacă două sesiuni partajează ACELAȘI obiect StreamQuality.
        /// Demonstrează că Flyweight returnează aceeași referință.
        /// </summary>
        public bool SharesQualityWith(StreamingSession other)
        {
            return ReferenceEquals(_quality, other._quality);
        }
    }
}
