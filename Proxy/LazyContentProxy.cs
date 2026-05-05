using System;
using StreamingPlatform.Models;

namespace StreamingPlatform.Proxy
{
    /// <summary>
    /// VIRTUAL PROXY — Lazy loading pentru conținuturi costisitoare.
    ///
    /// Problema: Afișăm o grilă cu 200 de filme. Dacă am crea
    /// RealContentPlayer pentru fiecare → 200 obiecte grele în memorie,
    /// chiar dacă utilizatorul vizionează poate 1-2.
    ///
    /// Soluția: LazyContentProxy creează RealContentPlayer NUMAI
    /// la primul apel Play(). GetInfo() funcționează fără să încarce realul.
    ///
    /// Clientul nu vede diferența — interfața IContentPlayer e identică.
    /// </summary>
    public class LazyContentProxy : IContentPlayer
    {
        // Datele minime — disponibile imediat, fără a crea subiectul real
        private readonly MediaContent _content;
        private readonly string _title;
        private readonly string _description;

        // Subiectul real — creat LAZY la primul Play()
        private RealContentPlayer? _realPlayer;
        private bool _isLoaded = false;

        public LazyContentProxy(MediaContent content)
        {
            _content     = content ?? throw new ArgumentNullException(nameof(content));
            _title       = content.Title;
            _description = content.Description;
        }

        public string GetTitle() => _title;

        /// <summary>
        /// GetInfo nu necesită subiectul real — returnează date minime instant.
        /// </summary>
        public string GetInfo()
        {
            if (!_isLoaded)
            {
                return $"[Virtual Proxy] '{_title}'\n" +
                       $"   {_description}\n" +
                       $"   (Apasă Play pentru a încărca datele complete)";
            }
            return _realPlayer!.GetInfo();
        }

        public bool CanUserAccess(string userName) => true;

        /// <summary>
        /// Play declanșează crearea lazy a subiectului real.
        /// Prima apelare → inițializare + redare.
        /// Apelări ulterioare → direct redare.
        /// </summary>
        public string Play(string userName)
        {
            if (!_isLoaded)
            {
                Console.WriteLine($"      [Virtual Proxy] Prima accesare — inițializez '{_title}'...");
                _realPlayer = new RealContentPlayer(_content);
                _isLoaded   = true;
                Console.WriteLine($"      [Virtual Proxy] '{_title}' inițializat și gata de redare.");
            }
            else
            {
                Console.WriteLine($"      [Virtual Proxy] '{_title}' deja inițializat — redare directă.");
            }

            return _realPlayer!.Play(userName);
        }

        public bool IsLoaded => _isLoaded;
    }
}
