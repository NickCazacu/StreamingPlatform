using System;

namespace StreamingPlatform.Models.Db
{
    /// <summary>
    /// Mapează tabelul dbo.Series din StreamZoneDB.
    /// </summary>
    public class SeriesEntity
    {
        public int SeriesId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string ContentRating { get; set; } = string.Empty;
        public string Creator { get; set; } = string.Empty;
        public int SeasonsCount { get; set; }
        public int EpisodesCount { get; set; }
        public int EpisodeDuration { get; set; }
        public bool IsCompleted { get; set; }
        public decimal AverageRating { get; set; }
        public bool IsMoldovan { get; set; }
        public string? PosterUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
