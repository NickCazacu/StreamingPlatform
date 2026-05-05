namespace StreamingPlatform.Behavioral.Command
{
    public class RemoveFromWatchlistCommand : ICommand
    {
        private readonly Watchlist _watchlist;
        private readonly string _contentTitle;

        public string Description => $"Elimină \"{_contentTitle}\" din watchlist-ul lui {_watchlist.UserName}";

        public RemoveFromWatchlistCommand(Watchlist watchlist, string contentTitle)
        {
            _watchlist = watchlist;
            _contentTitle = contentTitle;
        }

        public void Execute() => _watchlist.Remove(_contentTitle);
        public void Undo() => _watchlist.Add(_contentTitle);
    }
}
