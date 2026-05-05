using System;

namespace StreamingPlatform.Bridge
{
    /// <summary>
    /// CONCRETE IMPLEMENTOR — Tabletă.
    /// Portabilitate + ecran mai mare decât mobilul.
    /// </summary>
    public class TabletRenderer : IDeviceRenderer
    {
        private readonly string _model;

        public TabletRenderer(string model = "iPad") => _model = model;

        public void RenderVideo(string title, string quality, bool hasSubtitles)
        {
            string subs = hasSubtitles ? " | Subtitrări: ON" : "";
            Console.WriteLine($"      [📲 Tabletă/{_model}] Redare video: '{title}' " +
                              $"@ {quality} | AMOLED | Mod peisaj/portret{subs}");
        }

        public void RenderAudio(string title, string audioFormat)
        {
            Console.WriteLine($"      [📲 Tabletă/{_model}] Redare audio: '{title}' " +
                              $"| Format: {audioFormat} | Stereo dual-speaker");
        }

        public void RenderLiveStream(string channelName, int viewerCount, bool isHD)
        {
            string hd = isHD ? "1080p" : "720p";
            Console.WriteLine($"      [📲 Tabletă/{_model}] Live: '{channelName}' " +
                              $"| {viewerCount:N0} privitori | {hd} | Picture-in-Picture");
        }

        public string GetDeviceName()   => $"Tabletă ({_model})";
        public string GetCapabilities() => "Max 2K | H.264/H.265 | Touch | PiP | Portret/Peisaj";
    }
}
