using System;

namespace StreamingPlatform.Auth
{
    /// <summary>
    /// Mapează tabelul dbo.WatchEvents — fiecare vizionare e înregistrată
    /// pentru a calcula limitele zilnice (Free/Standard).
    /// </summary>
    public class WatchEvent
    {
        public int WatchEventId { get; set; }
        public int UserId { get; set; }
        public string ContentTitle { get; set; } = string.Empty;
        public string ContentType { get; set; } = "Movie";  // Movie | Series | Documentary
        public string? Quality { get; set; }
        public DateTime WatchedAt { get; set; } = DateTime.UtcNow;

        public Account? Account { get; set; }
    }
}
