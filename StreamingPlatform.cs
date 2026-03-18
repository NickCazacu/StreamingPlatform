using System;
using StreamingPlatform.Interfaces;

namespace StreamingPlatform.Services
{
    public class StreamingPlatform
    {
        private readonly IUserService _userService;
        private readonly IContentService _contentService;

        public StreamingPlatform(IUserService userService, IContentService contentService)
        {
            _userService = userService;
            _contentService = contentService;
        }

        public void PlayContent(int userId, int contentId)
        {
            _userService.WatchContent(userId, contentId);
            string result = _contentService.PlayContent(contentId);
            Console.WriteLine(result);
        }

        public void RateContent(int contentId, double rating)
        {
            _contentService.RateContent(contentId, rating);
        }

        public void AddToFavorites(int userId, int contentId)
        {
            _userService.AddToFavorites(userId, contentId);
        }
    }
}
