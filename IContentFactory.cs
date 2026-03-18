using StreamingPlatform.Models;

namespace StreamingPlatform.Interfaces
{
    public interface IContentFactory
    {
        MediaContent CreateContent(string title, string description, Genre genre, ContentRating rating);
    }
}
