namespace StreamingPlatform.Behavioral.Command
{
    public class RateContentCommand : ICommand
    {
        private readonly Watchlist _watchlist;
        private readonly string _contentTitle;
        private readonly double _newRating;
        private double _previousRating;
        private bool _hadPreviousRating;

        public string Description => $"Rating {_newRating}/5 pentru \"{_contentTitle}\" de {_watchlist.UserName}";

        public RateContentCommand(Watchlist watchlist, string contentTitle, double newRating)
        {
            _watchlist = watchlist;
            _contentTitle = contentTitle;
            _newRating = newRating;
        }

        public void Execute()
        {
            _hadPreviousRating = _watchlist.HasRating(_contentTitle);
            if (_hadPreviousRating)
                _previousRating = _watchlist.Ratings[_contentTitle];
            _watchlist.SetRating(_contentTitle, _newRating);
        }

        public void Undo()
        {
            if (_hadPreviousRating)
                _watchlist.SetRating(_contentTitle, _previousRating);
            else
                _watchlist.RemoveRating(_contentTitle);
        }
    }
}
