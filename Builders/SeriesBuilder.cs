using StreamingPlatform.Models;
using StreamingPlatform.Interfaces;

namespace StreamingPlatform.Builders
{
    public class SeriesBuilder : IContentBuilder<Series>
    {
        private string _title = "Untitled Series";
        private string _description = "";
        private Genre _genre = Genre.Drama;
        private ContentRating _rating = ContentRating.PG;
        private string _creator = "Unknown";
        private int _seasons = 1;
        private int _episodes = 10;
        private int _episodeDuration = 45;
        private bool _isCompleted = false;

        public SeriesBuilder SetTitle(string title)
        {
            _title = title;
            return this;
        }

        public SeriesBuilder SetDescription(string description)
        {
            _description = description;
            return this;
        }

        public SeriesBuilder SetGenre(Genre genre)
        {
            _genre = genre;
            return this;
        }

        public SeriesBuilder SetRating(ContentRating rating)
        {
            _rating = rating;
            return this;
        }

        public SeriesBuilder SetCreator(string creator)
        {
            _creator = creator;
            return this;
        }

        public SeriesBuilder SetSeasons(int seasons)
        {
            _seasons = seasons;
            return this;
        }

        public SeriesBuilder SetEpisodes(int episodes)
        {
            _episodes = episodes;
            return this;
        }

        public SeriesBuilder SetEpisodeDuration(int minutes)
        {
            _episodeDuration = minutes;
            return this;
        }

        public SeriesBuilder MarkAsCompleted()
        {
            _isCompleted = true;
            return this;
        }

        public SeriesBuilder MarkAsOngoing()
        {
            _isCompleted = false;
            return this;
        }

        public Series Build()
        {
            var series = new Series(_title, _description, _genre, _rating,
                _creator, _seasons, _episodes, _episodeDuration);
            series.IsCompleted = _isCompleted;
            return series;
        }

        public SeriesBuilder Reset()
        {
            _title = "Untitled Series";
            _description = "";
            _genre = Genre.Drama;
            _rating = ContentRating.PG;
            _creator = "Unknown";
            _seasons = 1;
            _episodes = 10;
            _episodeDuration = 45;
            _isCompleted = false;
            return this;
        }

        IContentBuilder<Series> IContentBuilder<Series>.SetTitle(string title) => SetTitle(title);
        IContentBuilder<Series> IContentBuilder<Series>.SetDescription(string description) => SetDescription(description);
        IContentBuilder<Series> IContentBuilder<Series>.SetGenre(Genre genre) => SetGenre(genre);
        IContentBuilder<Series> IContentBuilder<Series>.SetRating(ContentRating rating) => SetRating(rating);
        IContentBuilder<Series> IContentBuilder<Series>.Reset() => Reset();
    }
}