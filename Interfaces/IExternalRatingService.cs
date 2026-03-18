namespace StreamingPlatform.Interfaces
{
    /// <summary>
    /// Interfața comună (Target) pentru serviciile de rating extern.
    /// Toate serviciile externe trebuie adaptate la acest format
    /// pentru a fi folosite uniform în platformă.
    /// </summary>
    public interface IExternalRatingService
    {
        string GetServiceName();
        double GetRating(string contentTitle);
        string GetReview(string contentTitle);
        bool IsAvailable();
    }
}