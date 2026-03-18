using System;
using System.Collections.Generic;

namespace StreamingPlatform.Models
{
    public class User : Entity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public Subscription Subscription { get; }
        public List<int> WatchHistory { get; }
        public List<int> Favorites { get; }

        public User(string name, string email, int age)
        {
            Name = name;
            Email = email;
            Age = age;
            Subscription = new Subscription();
            WatchHistory = new List<int>();
            Favorites = new List<int>();
        }

        public void AddToHistory(int contentId)
        {
            if (!WatchHistory.Contains(contentId))
                WatchHistory.Add(contentId);
        }

        public void AddToFavorites(int contentId)
        {
            if (!Favorites.Contains(contentId))
                Favorites.Add(contentId);
        }

        public void RemoveFromFavorites(int contentId)
        {
            Favorites.Remove(contentId);
        }

        public bool CanWatch(ContentRating rating)
        {
            return rating switch
            {
                ContentRating.PG => true,
                ContentRating.PG13 => Age >= 13,
                ContentRating.R => Age >= 17,
                _ => true
            };
        }

        public override string GetInfo()
        {
            return $"Utilizator: {Name}\n" +
                   $"   Email: {Email}\n" +
                   $"   Vârstă: {Age}\n" +
                   $"   Abonament: {Subscription.GetInfo()}\n" +
                   $"   Vizionate: {WatchHistory.Count}\n" +
                   $"   Favorite: {Favorites.Count}";
        }
    }
}
