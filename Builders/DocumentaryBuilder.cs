using StreamingPlatform.Models;
using StreamingPlatform.Interfaces;

namespace StreamingPlatform.Builders
{
    public class DocumentaryBuilder : IContentBuilder<Documentary>
    {
        private string _title = "Untitled Documentary";
        private string _description = "";
        private Genre _genre = Genre.Documentary;
        private ContentRating _rating = ContentRating.G;
        private int _durationMinutes = 60;
        private string _topic = "General";
        private string _narrator = "Unknown";
        private bool _isEducational = true;

        public DocumentaryBuilder SetTitle(string title)
        {
            _title = title;
            return this;
        }

        public DocumentaryBuilder SetDescription(string description)
        {
            _description = description;
            return this;
        }

        public DocumentaryBuilder SetGenre(Genre genre)
        {
            _genre = genre;
            return this;
        }

        public DocumentaryBuilder SetRating(ContentRating rating)
        {
            _rating = rating;
            return this;
        }

        public DocumentaryBuilder SetDuration(int minutes)
        {
            _durationMinutes = minutes;
            return this;
        }

        public DocumentaryBuilder SetTopic(string topic)
        {
            _topic = topic;
            return this;
        }

        public DocumentaryBuilder SetNarrator(string narrator)
        {
            _narrator = narrator;
            return this;
        }

        public DocumentaryBuilder MarkAsEducational()
        {
            _isEducational = true;
            return this;
        }

        public DocumentaryBuilder MarkAsEntertainment()
        {
            _isEducational = false;
            return this;
        }

        public Documentary Build()
        {
            var doc = new Documentary(_title, _description, _genre, _rating,
                _durationMinutes, _topic, _narrator);
            doc.IsEducational = _isEducational;
            return doc;
        }

        public DocumentaryBuilder Reset()
        {
            _title = "Untitled Documentary";
            _description = "";
            _genre = Genre.Documentary;
            _rating = ContentRating.G;
            _durationMinutes = 60;
            _topic = "General";
            _narrator = "Unknown";
            _isEducational = true;
            return this;
        }

        IContentBuilder<Documentary> IContentBuilder<Documentary>.SetTitle(string title) => SetTitle(title);
        IContentBuilder<Documentary> IContentBuilder<Documentary>.SetDescription(string description) => SetDescription(description);
        IContentBuilder<Documentary> IContentBuilder<Documentary>.SetGenre(Genre genre) => SetGenre(genre);
        IContentBuilder<Documentary> IContentBuilder<Documentary>.SetRating(ContentRating rating) => SetRating(rating);
        IContentBuilder<Documentary> IContentBuilder<Documentary>.Reset() => Reset();
    }
}