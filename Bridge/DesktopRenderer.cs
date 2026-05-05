using System;

namespace StreamingPlatform.Bridge
{
    /// <summary>
    /// CONCRETE IMPLEMENTOR — Calculator / Laptop (browser).
    /// Resurse bogate, ecran mare, mouse și tastatură.
    /// </summary>
    public class DesktopRenderer : IDeviceRenderer
    {
        private readonly string _browser;

        public DesktopRenderer(string browser = "Chrome") => _browser = browser;

        public void RenderVideo(string title, string quality, bool hasSubtitles)
        {
            string subs = hasSubtitles ? " | Subtitrări disponibile" : "";
            Console.WriteLine($"      [💻 Desktop/{_browser}] Redare video: '{title}' " +
                              $"@ {quality} | Fullscreen | Controale mouse{subs}");
        }

        public void RenderAudio(string title, string audioFormat)
        {
            Console.WriteLine($"      [💻 Desktop/{_browser}] Redare audio: '{title}' " +
                              $"| Format: {audioFormat} | Sistem audio 5.1 Surround");
        }

        public void RenderLiveStream(string channelName, int viewerCount, bool isHD)
        {
            Console.WriteLine($"      [💻 Desktop/{_browser}] Live: '{channelName}' " +
                              $"| {viewerCount:N0} privitori | {'H' + (isHD ? "D 1080p" : "D 720p")} | Chat lateral activ");
        }

        public string GetDeviceName()   => $"Desktop ({_browser})";
        public string GetCapabilities() => "Max 4K | H.265/AV1 | Multi-tab | Extensii | Fullscreen";
    }
}
