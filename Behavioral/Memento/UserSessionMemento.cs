using System;

namespace StreamingPlatform.Behavioral.Memento
{
    public class UserSessionMemento
    {
        public string GenreFilter { get; }
        public string SortBy { get; }
        public string SearchQuery { get; }
        public int CurrentPage { get; }
        public DateTime SavedAt { get; }

        internal UserSessionMemento(string genreFilter, string sortBy, string searchQuery, int currentPage)
        {
            GenreFilter = genreFilter;
            SortBy = sortBy;
            SearchQuery = searchQuery;
            CurrentPage = currentPage;
            SavedAt = DateTime.Now;
        }

        public override string ToString() =>
            $"Stare din {SavedAt:HH:mm:ss} — Filtru: {GenreFilter}, Sortare: {SortBy}, Căutare: \"{SearchQuery}\", Pagina: {CurrentPage}";
    }
}
