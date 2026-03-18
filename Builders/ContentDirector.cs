using StreamingPlatform.Models;

namespace StreamingPlatform.Builders
{
    public class ContentDirector
    {
        public Movie BuildBlockbusterMovie(MovieBuilder builder)
        {
            return builder
                .SetGenre(Genre.Action)
                .SetRating(ContentRating.PG13)
                .SetDuration(150)
                .Build();
        }

        public Movie BuildShortFilm(MovieBuilder builder)
        {
            return builder
                .SetGenre(Genre.Drama)
                .SetRating(ContentRating.PG)
                .SetDuration(30)
                .Build();
        }

        public Movie BuildHorrorMovie(MovieBuilder builder)
        {
            return builder
                .SetGenre(Genre.Horror)
                .SetRating(ContentRating.R)
                .SetDuration(100)
                .Build();
        }

        public Series BuildMiniSeries(SeriesBuilder builder)
        {
            return builder
                .SetSeasons(1)
                .SetEpisodes(6)
                .SetEpisodeDuration(55)
                .MarkAsCompleted()
                .Build();
        }

        public Series BuildLongRunningSeries(SeriesBuilder builder)
        {
            return builder
                .SetSeasons(10)
                .SetEpisodes(220)
                .SetEpisodeDuration(42)
                .MarkAsOngoing()
                .Build();
        }

        public Documentary BuildNatureDocumentary(DocumentaryBuilder builder)
        {
            return builder
                .SetGenre(Genre.Documentary)
                .SetRating(ContentRating.G)
                .SetDuration(50)
                .SetTopic("Nature")
                .MarkAsEducational()
                .Build();
        }

        public Documentary BuildHistoryDocumentary(DocumentaryBuilder builder)
        {
            return builder
                .SetGenre(Genre.Documentary)
                .SetRating(ContentRating.PG)
                .SetDuration(90)
                .SetTopic("History")
                .MarkAsEducational()
                .Build();
        }
    }
}
