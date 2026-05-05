using System;
using StreamingPlatform.Models;

namespace StreamingPlatform.Proxy
{
    /// <summary>
    /// REAL SUBJECT — Redă efectiv conținutul.
    /// Nu cunoaște nimic despre autorizare, vârstă sau abonamente.
    /// Responsabilitatea unică: redă conținut și returnează rezultatul.
    /// </summary>
    public class RealContentPlayer : IContentPlayer
    {
        private readonly MediaContent _content;

        public RealContentPlayer(MediaContent content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public string Play(string userName)
        {
            string result = _content.Play();
            return $"▶ {result}";
        }

        public string GetInfo()             => _content.GetInfo();
        public string GetTitle()            => _content.Title;
        public ContentRating GetRating()    => _content.Rating;

        // Real subject permite mereu accesul — Proxy-ul decide
        public bool CanUserAccess(string userName) => true;
    }
}
