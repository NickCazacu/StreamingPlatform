using System;

namespace StreamingPlatform.Bridge
{
    /// <summary>
    /// CONCRETE IMPLEMENTOR — Smart TV.
    /// Ecranul cel mai mare, cea mai bună calitate, telecomandă.
    /// </summary>
    public class SmartTVRenderer : IDeviceRenderer
    {
        private readonly string _brand;
        private readonly bool _dolbyVision;

        public SmartTVRenderer(string brand = "Samsung", bool dolbyVision = true)
        {
            _brand       = brand;
            _dolbyVision = dolbyVision;
        }

        public void RenderVideo(string title, string quality, bool hasSubtitles)
        {
            string tvQ  = (quality is "4K" or "4KUHD") && _dolbyVision
                ? $"{quality} + Dolby Vision" : quality;
            string subs = hasSubtitles ? " | Subtitrări pe ecran mare" : "";
            Console.WriteLine($"      [📺 Smart TV/{_brand}] Redare video: '{title}' " +
                              $"@ {tvQ} | Dolby Atmos | Telecomandă{subs}");
        }

        public void RenderAudio(string title, string audioFormat)
        {
            Console.WriteLine($"      [📺 Smart TV/{_brand}] Redare audio: '{title}' " +
                              $"| Format: {audioFormat} | Soundbar/Home Cinema conectat");
        }

        public void RenderLiveStream(string channelName, int viewerCount, bool isHD)
        {
            string quality = isHD ? "4K UHD" : "1080p";
            Console.WriteLine($"      [📺 Smart TV/{_brand}] Live: '{channelName}' " +
                              $"| {viewerCount:N0} privitori | {quality} | Ecran complet 65\"");
        }

        public string GetDeviceName()   => $"Smart TV ({_brand})";
        public string GetCapabilities() => "Max 4K UHD | HDR10+ | Dolby Vision | Dolby Atmos | HDMI 2.1";
    }
}
