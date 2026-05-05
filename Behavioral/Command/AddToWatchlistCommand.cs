namespace StreamingPlatform.Behavioral.Command
{
    public class AddToWatchlistCommand : ICommand
    {
        private readonly Watchlist _watchlist;
        private readonly string _contentTitle;

        public string Description => $"Adaugă \"{_contentTitle}\" în watchlist-ul lui {_watchlist.UserName}";

        public AddToWatchlistCommand(Watchlist watchlist, string contentTitle)
        {
            _watchlist = watchlist;
            _contentTitle = contentTitle;
        }

        public void Execute() => _watchlist.Add(_contentTitle);
        public void Undo() => _watchlist.Remove(_contentTitle);
    }
}
