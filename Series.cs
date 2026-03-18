using StreamingPlatform.Interfaces;

namespace StreamingPlatform.Models
{
    public class Series : MediaContent, IPrototype<Series>
    {
        public string Creator { get; set; }
        public int SeasonsCount { get; set; }
        public int EpisodesCount { get; set; }
        public int AverageEpisodeDuration { get; set; }
        public bool IsCompleted { get; set; }

        public Series(string title, string description, Genre genre, ContentRating rating,
                     string creator, int seasons, int episodes, int avgEpisodeDuration)
            : base(title, description, genre, rating)
        {
            Creator = creator;
            SeasonsCount = seasons;
            EpisodesCount = episodes;
            AverageEpisodeDuration = avgEpisodeDuration;
            IsCompleted = false;
        }

        public override string Play()
        {
            IncrementViews();
            return $"Se redă serialul '{Title}' - Sezonul 1, Episodul 1";
        }

        public override int GetDuration()
        {
            return EpisodesCount * AverageEpisodeDuration;
        }

        public override string GetInfo()
        {
            string status = IsCompleted ? "Finalizat" : "În desfășurare";
            int totalHours = GetDuration() / 60;

            return $"Serial: {Title}\n" +
                   $"   Gen: {Genre}, Rating: {Rating}\n" +
                   $"   Creator: {Creator}\n" +
                   $"   Sezoane: {SeasonsCount}, Episoade: {EpisodesCount}\n" +
                   $"   Durată totală: ~{totalHours}h\n" +
                   $"   Vizualizări: {ViewsCount}, Scor: {AverageRating}/5\n" +
                   $"   Status: {status}";
        }

        public Series ShallowClone()
        {
            return (Series)this.MemberwiseClone();
        }

        public Series DeepClone()
        {
            var clone = new Series(Title, Description, Genre, Rating,
                Creator, SeasonsCount, EpisodesCount, AverageEpisodeDuration);
            clone.IsCompleted = IsCompleted;
            return clone;
        }
    }
}
