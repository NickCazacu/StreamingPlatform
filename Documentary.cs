using StreamingPlatform.Interfaces;

namespace StreamingPlatform.Models
{
    public class Documentary : MediaContent, IPrototype<Documentary>
    {
        public int DurationMinutes { get; set; }
        public string Topic { get; set; }
        public string Narrator { get; set; }
        public bool IsEducational { get; set; }

        public Documentary(string title, string description, Genre genre, ContentRating rating,
                          int durationMinutes, string topic, string narrator)
            : base(title, description, genre, rating)
        {
            DurationMinutes = durationMinutes;
            Topic = topic;
            Narrator = narrator;
            IsEducational = true;
        }

        public override string Play()
        {
            IncrementViews();
            int hours = DurationMinutes / 60;
            int minutes = DurationMinutes % 60;
            return $"Se redă documentarul '{Title}' ({hours}h {minutes}m) - Narare: {Narrator}";
        }

        public override int GetDuration() => DurationMinutes;

        public override string GetInfo()
        {
            return $"Documentar: {Title}\n" +
                   $"   Gen: {Genre}, Rating: {Rating}\n" +
                   $"   Subiect: {Topic}\n" +
                   $"   Narrator: {Narrator}\n" +
                   $"   Durată: {DurationMinutes} min\n" +
                   $"   Vizualizări: {ViewsCount}, Scor: {AverageRating}/5\n" +
                   $"   Educațional: {(IsEducational ? "Da" : "Nu")}";
        }

        public Documentary ShallowClone()
        {
            return (Documentary)this.MemberwiseClone();
        }

        public Documentary DeepClone()
        {
            var clone = new Documentary(Title, Description, Genre, Rating,
                DurationMinutes, Topic, Narrator);
            clone.IsEducational = IsEducational;
            return clone;
        }
    }
}
