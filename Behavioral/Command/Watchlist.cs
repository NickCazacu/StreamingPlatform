using System.Collections.Generic;

namespace StreamingPlatform.Behavioral.Command
{
    public class Watchlist
    {
        private readonly List<string> _items = new();
        private readonly Dictionary<string, double> _ratings = new();

        public string UserName { get; }
        public IReadOnlyList<string> Items => _items.AsReadOnly();
        public IReadOnlyDictionary<string, double> Ratings => _ratings;

        public Watchlist(string userName)
        {
            UserName = userName;
        }

        public bool Add(string title)
        {
            if (_items.Contains(title)) return false;
            _items.Add(title);
            return true;
        }

        public bool Remove(string title) => _items.Remove(title);

        public void SetRating(string title, double rating) => _ratings[title] = rating;

        public void RemoveRating(string title) => _ratings.Remove(title);

        public bool HasRating(string title) => _ratings.ContainsKey(title);
    }
}
