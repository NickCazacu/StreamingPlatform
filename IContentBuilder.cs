using StreamingPlatform.Models;

namespace StreamingPlatform.Interfaces
{
    public interface IContentBuilder<T> where T : MediaContent
    {
        IContentBuilder<T> SetTitle(string title);
        IContentBuilder<T> SetDescription(string description);
        IContentBuilder<T> SetGenre(Genre genre);
        IContentBuilder<T> SetRating(ContentRating rating);
        T Build();
        IContentBuilder<T> Reset();
    }
}
