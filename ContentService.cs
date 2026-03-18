using System;
using StreamingPlatform.Interfaces;
using StreamingPlatform.Models;

namespace StreamingPlatform.Services
{
    public class ContentService : IContentService
    {
        private readonly IRepository<MediaContent> _repository;

        public ContentService(IRepository<MediaContent> repository)
        {
            _repository = repository;
        }

        public void AddContent(MediaContent content)
        {
            _repository.Add(content);
        }

        public MediaContent GetContent(int id)
        {
            return _repository.GetById(id);
        }

        public void RateContent(int contentId, double rating)
        {
            var content = _repository.GetById(contentId);
            if (content == null)
                throw new InvalidOperationException("Content not found");

            content.AddRating(rating);
        }

        public string PlayContent(int contentId)
        {
            var content = _repository.GetById(contentId);
            if (content == null)
                throw new InvalidOperationException("Content not found");

            return content.Play();
        }
    }
}
