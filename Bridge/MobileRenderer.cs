using System;

namespace StreamingPlatform.Bridge
{
    /// <summary>
    /// CONCRETE IMPLEMENTOR — Dispozitiv mobil (telefon).
    /// Optimizat pentru baterie, ecran mic, conexiune variabilă.
    /// </summary>
    public class MobileRenderer : IDeviceRenderer
    {
        private readonly string _os;

        public MobileRenderer(string os = "Android") => _os = os;

        public void RenderVideo(string title, string quality, bool hasSubtitles)
        {
            string mobileQ = quality is "4K" or "4KUHD" ? "1080p (adaptat mobil)" : quality;
            string subs    = hasSubtitles ? " | Subtitrări: ON" : "";
            Console.WriteLine($"      [📱 Mobile/{_os}] Redare video: '{title}' " +
                              $"@ {mobileQ} | Mod economie baterie{subs}");
        }

        public void RenderAudio(string title, string audioFormat)
        {
            Console.WriteLine($"      [📱 Mobile/{_os}] Redare audio: '{title}' " +
                              $"| Format: {audioFormat} | Difuzor/Căști Bluetooth");
        }

        public void RenderLiveStream(string channelName, int viewerCount, bool isHD)
        {
            string hd = isHD ? "HD" : "SD (economie date)";
            Console.WriteLine($"      [📱 Mobile/{_os}] Live: '{channelName}' " +
                              $"| {viewerCount:N0} privitori | {hd} | Latență adaptivă");
        }

        public string GetDeviceName()   => $"Mobile ({_os})";
        public string GetCapabilities() => "Max 1080p | H.264 | Touch | Economie baterie";
    }
}
