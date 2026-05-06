using System;

namespace StreamingPlatform.Models.Db
{
    /// <summary>
    /// Mapează tabelul dbo.Movies din StreamZoneDB.
    /// Folosit pentru persistența și interogarea filmelor din baza de date.
    /// </summary>
    public class MovieEntity
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;          // "Drama", "SciFi", ...
        public string ContentRating { get; set; } = string.Empty;  // "G", "PG", "PG13", "R"
        public int DurationMinutes { get; set; }
        public string Director { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
        public bool IsMoldovan { get; set; }
        public string? PosterUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
