namespace StreamingPlatform.Flyweight
{
    // ============================================================
    // FLYWEIGHT PATTERN
    // ============================================================
    // Problema: Platforma StreamZone gestionează milioane de sesiuni
    // de streaming simultane. Fiecare sesiune are o configurație de
    // calitate video (rezoluție, bitrate, codec, format).
    //
    // Dacă am crea un obiect StreamQuality NOU pentru fiecare sesiune:
    //   1.000.000 sesiuni × ~200 bytes/obiect = 200 MB RAM doar pentru calități!
    //
    // Observație: Calitățile sunt FINITE și REPETITIVE (360p, 720p, 1080p, 4K...).
    // Milioane de sesiuni la "1080p" ar crea același obiect de un milion de ori!
    //
    // Soluția — Flyweight:
    //   - Starea INTRINSECĂ (resolution, bitrate, codec, format, HDR, FPS)
    //     e IMUTABILĂ și PARTAJATĂ între toate sesiunile.
    //   - Starea EXTRINSECĂ (utilizator, conținut, dispozitiv, timestamp)
    //     e unică per sesiune și se transmite ca PARAMETRU, nu se stochează.
    //
    // Rezultat: 1.000.000 sesiuni → doar 6 obiecte StreamQuality în memorie!
    // ============================================================

    /// <summary>
    /// FLYWEIGHT — Configurație de calitate video partajată.
    /// Obiectul este IMUTABIL — starea intrinsecă nu se schimbă niciodată.
    /// Poate fi refolosit de oricâte sesiuni simultan.
    /// </summary>
    public class StreamQuality
    {
        // ── Stare INTRINSECĂ (partajată, imutabilă) ──────────────────────
        public string Resolution { get; }    // "720p", "1080p", "4K"
        public int BitrateKbps { get; }      // Kilobiti per secundă
        public string Codec { get; }         // "H.264", "H.265", "AV1"
        public string Format { get; }        // "HLS", "DASH", "MP4"
        public bool HDRSupport { get; }      // High Dynamic Range
        public int MaxFPS { get; }           // Cadre per secundă

        public StreamQuality(string resolution, int bitrateKbps, string codec,
                             string format, bool hdrSupport, int maxFps)
        {
            Resolution  = resolution;
            BitrateKbps = bitrateKbps;
            Codec       = codec;
            Format      = format;
            HDRSupport  = hdrSupport;
            MaxFPS      = maxFps;
        }

        /// <summary>
        /// Redă conținut folosind starea extrinsecă transmisă ca parametru.
        /// Starea extrinsecă (user, content, device) NU este stocată în obiect —
        /// aceasta e esența Flyweight.
        /// </summary>
        public string Render(string userName, string contentTitle, string deviceType)
        {
            return $"[{Resolution}/{MaxFPS}fps/{Codec}] " +
                   $"'{contentTitle}' → {deviceType} " +
                   $"(utilizator: {userName}, bitrate: {BitrateKbps}kbps, HDR: {HDRSupport})";
        }

        public string GetDescription()
        {
            string hdr = HDRSupport ? " | HDR10" : "";
            return $"{Resolution} | {BitrateKbps}kbps | {Codec} | {Format}{hdr} | {MaxFPS}fps";
        }

        public override string ToString() => Resolution;
    }
}
