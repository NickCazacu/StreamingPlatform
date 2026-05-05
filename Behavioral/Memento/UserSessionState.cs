namespace StreamingPlatform.Behavioral.Memento
{
    public class UserSessionState
    {
        public string UserName { get; }
        public string GenreFilter { get; set; } = "All";
        public string SortBy { get; set; } = "Rating";
        public string SearchQuery { get; set; } = "";
        public int CurrentPage { get; set; } = 1;

        public UserSessionState(string userName)
        {
            UserName = userName;
        }

        public UserSessionMemento SaveState()
        {
            return new UserSessionMemento(GenreFilter, SortBy, SearchQuery, CurrentPage);
        }

        public void RestoreState(UserSessionMemento memento)
        {
            GenreFilter = memento.GenreFilter;
            SortBy = memento.SortBy;
            SearchQuery = memento.SearchQuery;
            CurrentPage = memento.CurrentPage;
        }

        public override string ToString() =>
            $"[{UserName}] Filtru: {GenreFilter} | Sortare: {SortBy} | Căutare: \"{SearchQuery}\" | Pagina: {CurrentPage}";
    }
}
