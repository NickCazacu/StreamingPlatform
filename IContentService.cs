using StreamingPlatform.Models;

namespace StreamingPlatform.Interfaces
{
    public interface IContentService
    {
        void AddContent(MediaContent content);
        MediaContent GetContent(int id);
        void RateContent(int contentId, double rating);
        string PlayContent(int contentId);
    }
}
