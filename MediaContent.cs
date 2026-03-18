using System;

namespace StreamingPlatform.Models
{
    public abstract class MediaContent : Entity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Genre Genre { get; set; }
        public ContentRating Rating { get; set; }
        public int ViewsCount { get; private set; }
        public double AverageRating { get; private set; }

        private int _ratingsCount;

        protected MediaContent(string title, string description, Genre genre, ContentRating rating)
        {
            Title = title;
            Description = description;
            Genre = genre;
            Rating = rating;
            ViewsCount = 0;
            AverageRating = 0;
            _ratingsCount = 0;
        }

        public void IncrementViews()
        {
            ViewsCount++;
        }

        public void AddRating(double rating)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            double total = AverageRating * _ratingsCount + rating;
            _ratingsCount++;
            AverageRating = Math.Round(total / _ratingsCount, 2);
        }

        public abstract string Play();
        public abstract int GetDuration();
    }
}
