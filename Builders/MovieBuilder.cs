using System.Collections.Generic;
using StreamingPlatform.Models;
using StreamingPlatform.Interfaces;

namespace StreamingPlatform.Builders
{
    public class MovieBuilder : IContentBuilder<Movie>
    {
        private string _title = "Untitled Movie";
        private string _description = "";
        private Genre _genre = Genre.Action;
        private ContentRating _rating = ContentRating.PG;
        private int _durationMinutes = 90;
        private string _director = "Unknown";
        private List<string> _cast = new List<string>();

        public MovieBuilder SetTitle(string title)
        {
            _title = title;
            return this;
        }

        public MovieBuilder SetDescription(string description)
        {
            _description = description;
            return this;
        }

        public MovieBuilder SetGenre(Genre genre)
        {
            _genre = genre;
            return this;
        }

        public MovieBuilder SetRating(ContentRating rating)
        {
            _rating = rating;
            return this;
        }

        public MovieBuilder SetDuration(int minutes)
        {
            _durationMinutes = minutes;
            return this;
        }

        public MovieBuilder SetDirector(string director)
        {
            _director = director;
            return this;
        }

        public MovieBuilder AddCastMember(string actor)
        {
            if (!_cast.Contains(actor))
                _cast.Add(actor);
            return this;
        }

        public MovieBuilder AddCastMembers(params string[] actors)
        {
            foreach (var actor in actors)
                AddCastMember(actor);
            return this;
        }

        public Movie Build()
        {
            var movie = new Movie(_title, _description, _genre, _rating, _durationMinutes, _director);
            foreach (var actor in _cast)
                movie.AddCastMember(actor);
            return movie;
        }

        public MovieBuilder Reset()
        {
            _title = "Untitled Movie";
            _description = "";
            _genre = Genre.Action;
            _rating = ContentRating.PG;
            _durationMinutes = 90;
            _director = "Unknown";
            _cast = new List<string>();
            return this;
        }

        IContentBuilder<Movie> IContentBuilder<Movie>.SetTitle(string title) => SetTitle(title);
        IContentBuilder<Movie> IContentBuilder<Movie>.SetDescription(string description) => SetDescription(description);
        IContentBuilder<Movie> IContentBuilder<Movie>.SetGenre(Genre genre) => SetGenre(genre);
        IContentBuilder<Movie> IContentBuilder<Movie>.SetRating(ContentRating rating) => SetRating(rating);
        IContentBuilder<Movie> IContentBuilder<Movie>.Reset() => Reset();
    }
}