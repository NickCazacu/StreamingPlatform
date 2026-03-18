using System;
using System.Collections.Generic;
using StreamingPlatform.Models;
using StreamingPlatform.Interfaces;

namespace StreamingPlatform.Factories
{
    public abstract class ContentCreator : IContentFactory
    {
        private readonly List<MediaContent> _createdContent = new List<MediaContent>();

        public abstract MediaContent CreateContent(string title, string description,
            Genre genre, ContentRating rating);

        public MediaContent CreateAndRegister(string title, string description,
            Genre genre, ContentRating rating)
        {
            var content = CreateContent(title, description, genre, rating);
            _createdContent.Add(content);
            Console.WriteLine($"  [Factory] Creat: {content.GetType().Name} - '{title}'");
            return content;
        }

        public IReadOnlyList<MediaContent> GetCreatedContent() => _createdContent.AsReadOnly();
        public int GetCreatedCount() => _createdContent.Count;
    }

    public class MovieCreator : ContentCreator
    {
        private readonly string _defaultDirector;
        private readonly int _defaultDuration;

        public MovieCreator(string defaultDirector = "Unknown", int defaultDuration = 120)
        {
            _defaultDirector = defaultDirector;
            _defaultDuration = defaultDuration;
        }

        public override MediaContent CreateContent(string title, string description,
            Genre genre, ContentRating rating)
        {
            return new Movie(title, description, genre, rating, _defaultDuration, _defaultDirector);
        }

        public MediaContent CreateAndRegister(string title, string description,
            Genre genre, ContentRating rating, int durationMinutes, string director)
        {
            var movie = new Movie(title, description, genre, rating, durationMinutes, director);
            var field = typeof(ContentCreator)
                .GetField("_createdContent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ((List<MediaContent>)field.GetValue(this)).Add(movie);
            Console.WriteLine($"  [Factory] Creat: Movie - '{title}' (Regizor: {director}, {durationMinutes} min)");
            return movie;
        }
    }

    public class SeriesCreator : ContentCreator
    {
        private readonly string _defaultCreator;
        private readonly int _defaultSeasons;
        private readonly int _defaultEpisodes;
        private readonly int _defaultEpisodeDuration;

        public SeriesCreator(string defaultCreator = "Unknown", int defaultSeasons = 1,
            int defaultEpisodes = 10, int defaultEpisodeDuration = 45)
        {
            _defaultCreator = defaultCreator;
            _defaultSeasons = defaultSeasons;
            _defaultEpisodes = defaultEpisodes;
            _defaultEpisodeDuration = defaultEpisodeDuration;
        }

        public override MediaContent CreateContent(string title, string description,
            Genre genre, ContentRating rating)
        {
            return new Series(title, description, genre, rating,
                _defaultCreator, _defaultSeasons, _defaultEpisodes, _defaultEpisodeDuration);
        }

        public MediaContent CreateAndRegister(string title, string description,
            Genre genre, ContentRating rating, string creator, int seasons,
            int episodes, int avgEpisodeDuration)
        {
            var series = new Series(title, description, genre, rating,
                creator, seasons, episodes, avgEpisodeDuration);
            var field = typeof(ContentCreator)
                .GetField("_createdContent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ((List<MediaContent>)field.GetValue(this)).Add(series);
            Console.WriteLine($"  [Factory] Creat: Series - '{title}' (Creator: {creator}, {seasons} sez, {episodes} ep)");
            return series;
        }
    }


    public class DocumentaryCreator : ContentCreator
    {
        private readonly string _defaultNarrator;
        private readonly int _defaultDuration;

        public DocumentaryCreator(string defaultNarrator = "Unknown", int defaultDuration = 90)
        {
            _defaultNarrator = defaultNarrator;
            _defaultDuration = defaultDuration;
        }

        public override MediaContent CreateContent(string title, string description,
            Genre genre, ContentRating rating)
        {
            return new Documentary(title, description, genre, rating,
                _defaultDuration, title, _defaultNarrator);
        }

        public MediaContent CreateAndRegister(string title, string description,
            Genre genre, ContentRating rating, int durationMinutes,
            string topic, string narrator)
        {
            var doc = new Documentary(title, description, genre, rating,
                durationMinutes, topic, narrator);
            var field = typeof(ContentCreator)
                .GetField("_createdContent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ((List<MediaContent>)field.GetValue(this)).Add(doc);
            Console.WriteLine($"  [Factory] Creat: Documentary - '{title}' (Narrator: {narrator}, {durationMinutes} min)");
            return doc;
        }
    }

    public static class ContentFactoryProvider
    {
        public static ContentCreator GetFactory(ContentType type)
        {
            return type switch
            {
                ContentType.Movie => new MovieCreator(),
                ContentType.Series => new SeriesCreator(),
                ContentType.Documentary => new DocumentaryCreator(),
                _ => throw new ArgumentException($"Tip de conținut necunoscut: {type}")
            };
        }
    }
}
