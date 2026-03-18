using System;
using StreamingPlatform.Interfaces;
using StreamingPlatform.Models;

namespace StreamingPlatform.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<MediaContent> _contentRepository;

        public UserService(IRepository<User> userRepository, IRepository<MediaContent> contentRepository)
        {
            _userRepository = userRepository;
            _contentRepository = contentRepository;
        }

        public void AddUser(User user)
        {
            _userRepository.Add(user);
        }

        public User GetUser(int id)
        {
            return _userRepository.GetById(id);
        }

        public void WatchContent(int userId, int contentId)
        {
            var user = _userRepository.GetById(userId);
            var content = _contentRepository.GetById(contentId);

            if (user == null)
                throw new InvalidOperationException("User not found");
            if (content == null)
                throw new InvalidOperationException("Content not found");

            if (!user.CanWatch(content.Rating))
                throw new InvalidOperationException($"User cannot watch content rated {content.Rating}");

            user.AddToHistory(contentId);
        }

        public void AddToFavorites(int userId, int contentId)
        {
            var user = _userRepository.GetById(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            user.AddToFavorites(contentId);
        }
    }
}
