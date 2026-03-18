using System.Collections.Generic;
using StreamingPlatform.Interfaces;

namespace StreamingPlatform.Models
{
    public class Movie : MediaContent, IPrototype<Movie>
    {
        public int DurationMinutes { get; set; }
        public string Director { get; set; }
        public List<string> Cast { get; }

        public Movie(string title, string description, Genre genre,
                    ContentRating rating, int durationMinutes, string director)
            : base(title, description, genre, rating)
        {
            DurationMinutes = durationMinutes;
            Director = director;
            Cast = new List<string>();
        }

        public void AddCastMember(string actorName)
        {
            if (!Cast.Contains(actorName))
                Cast.Add(actorName);
        }

        public override string Play()
        {
            IncrementViews();
            int hours = DurationMinutes / 60;
            int minutes = DurationMinutes % 60;
            return $"Se redă filmul '{Title}' ({hours}h {minutes}m)";
        }

        public override int GetDuration() => DurationMinutes;

        public override string GetInfo()
        {
            return $"Film: {Title}\n" +
                   $"   Gen: {Genre}, Rating: {Rating}\n" +
                   $"   Regizor: {Director}\n" +
                   $"   Durată: {DurationMinutes} min\n" +
                   $"   Vizualizări: {ViewsCount}, Scor: {AverageRating}/5";
        }
        public Movie ShallowClone()
        {
            return (Movie)this.MemberwiseClone();
        }

        public Movie DeepClone()
        {
            var clone = new Movie(Title, Description, Genre, Rating, DurationMinutes, Director);
            foreach (var actor in Cast)
                clone.AddCastMember(actor);
            return clone;
        }
    }
}
