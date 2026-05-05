namespace StreamingPlatform.Proxy
{
    // ============================================================
    // PROXY PATTERN
    // ============================================================
    // Problema: Conținutul platformei StreamZone trebuie protejat:
    //   - Un film R-rated nu poate fi vizionat de minori (sub 17 ani)
    //   - Conținutul premium nu e accesibil cu abonament Free
    //   - Vrem să logăm toate accesele (permise și refuzate) pentru audit
    //   - Conținuturi grele (preview-uri HD, date externe) nu trebuie
    //     încărcate decât dacă utilizatorul chiar vizionează
    //
    // Soluție GREȘITĂ: Punem verificările direct în MediaContent.Play() →
    //   violăm Single Responsibility și Open/Closed.
    //
    // Soluția — Proxy:
    //   RealContentPlayer (subiectul real) nu știe nimic de securitate.
    //   ContentAccessProxy (protection proxy) se interpune și:
    //     1. Verifică autentificarea
    //     2. Verifică abonamentul vs rating-ul conținutului
    //     3. Verifică vârsta utilizatorului
    //     4. Loghează accesul (permis sau refuzat)
    //     5. Deleghează la subiectul real NUMAI dacă totul e OK
    //
    //   LazyContentProxy (virtual proxy):
    //     - Creează subiectul real NUMAI la primul Play()
    //     - GetInfo() returnează preview fără să încarce datele complete
    //
    // Clientul lucrează cu IContentPlayer — nu știe dacă e Proxy sau Real!
    // ============================================================

    /// <summary>
    /// SUBJECT interface — implementată atât de Proxy cât și de Real Subject.
    /// Clientul interacționează EXCLUSIV prin această interfață —
    /// nu știe dacă are în față un Proxy sau subiectul real.
    /// </summary>
    public interface IContentPlayer
    {
        /// <summary>Redă conținutul (Proxy verifică autorizarea mai întâi).</summary>
        string Play(string userName);

        /// <summary>Returnează informații (disponibil și fără autentificare).</summary>
        string GetInfo();

        /// <summary>Returnează titlul conținutului.</summary>
        string GetTitle();

        /// <summary>Verifică dacă utilizatorul are acces (fără a reda).</summary>
        bool CanUserAccess(string userName);
    }
}
