using System.Collections.Generic;

namespace StreamingPlatform.Interfaces
{
    /// <summary>
    /// Interfața Composite Pattern — componenta comună.
    /// Atât obiectele individuale (un film, un serial) cât și
    /// colecțiile (playlist, categorie) implementează aceeași interfață.
    /// Clientul le tratează UNIFORM — nu știe dacă e 1 element sau 100.
    /// </summary>
    public interface IMediaComponent
    {
        string GetName();
        string Display(string indent = "");
        int GetTotalDuration();
        int GetItemCount();
        double GetAverageRating();
        List<IMediaComponent> GetChildren();
    }
}