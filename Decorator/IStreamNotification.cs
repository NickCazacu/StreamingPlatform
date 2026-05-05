namespace StreamingPlatform.Decorator
{
    // ============================================================
    // DECORATOR PATTERN
    // ============================================================
    // Problema: Sistemul de notificări al platformei StreamZone
    // trebuie să trimită alerte prin mai multe canale:
    //   - Consolă/log intern (implicit)
    //   - Email
    //   - SMS
    //   - Push notification în aplicație
    //
    // Variante de combinații necesare:
    //   - Abonament expirat: Email + SMS + Push
    //   - Conținut nou: Email + Push
    //   - Stream pornit: doar consolă
    //   - Ofertă specială: SMS + Push
    //
    // Abordare GREȘITĂ: moștenire multiplă →
    //   BaseNotification, EmailNotification, SmsNotification,
    //   EmailSmsNotification, EmailPushNotification, SmsPushNotification,
    //   EmailSmsPushNotification... EXPLOZIE combinatorie!
    //
    // Soluția — Decorator:
    //   Fiecare decorator ÎNVELEȘTE un IStreamNotification și adaugă un canal.
    //   Se compun dinamic: new Push(new Sms(new Email(new Base())))
    //   → trimite prin toate 3 canalele, fără a crea clase combinatorie.
    //
    //   Clientul lucrează cu IStreamNotification indiferent de câte
    //   decoratoare sunt adăugate — interfața e identică.
    // ============================================================

    /// <summary>
    /// COMPONENT interface — implementată atât de componenta de bază
    /// cât și de toate decoratoarele.
    /// Clientul lucrează EXCLUSIV cu această interfață.
    /// </summary>
    public interface IStreamNotification
    {
        /// <summary>Trimite notificarea prin toate canalele configurate.</summary>
        void Send(string userName, string message);

        /// <summary>Returnează lista canalelor active (pentru audit/debug).</summary>
        string GetChannels();
    }
}
