using StreamingPlatform.Models;

namespace StreamingPlatform.Interfaces
{
    public interface IUserService
    {
        void AddUser(User user);
        User GetUser(int id);
        void WatchContent(int userId, int contentId);
        void AddToFavorites(int userId, int contentId);
    }
}
