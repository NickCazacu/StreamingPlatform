using System;

namespace StreamingPlatform.Models.Db
{
    /// <summary>
    /// Mapează tabelul dbo.Documentaries din StreamZoneDB.
    /// </summary>
    public class DocumentaryEntity
    {
        public int DocumentaryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Genre { get; set; } = "Documentary";
        public string ContentRating { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string Narrator { get; set; } = string.Empty;
        public bool IsEducational { get; set; } = true;
        public decimal AverageRating { get; set; }
        public bool IsMoldovan { get; set; }
        public string? PosterUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
