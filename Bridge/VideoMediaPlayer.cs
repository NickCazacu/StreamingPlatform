using System;

namespace StreamingPlatform.Bridge
{
    /// <summary>
    /// REFINED ABSTRACTION — Player pentru conținut Video (film/serial).
    /// Adaugă logică specifică: subtitrări, episoade, comenzi rapide.
    /// Delegă randarea actuală la dispozitivul curent prin bridge.
    /// </summary>
    public class VideoMediaPlayer : MediaPlayerBase
    {
        private bool _subtitlesEnabled;
        private readonly string _subtitleLanguage;

        public VideoMediaPlayer(IDeviceRenderer device, bool subtitles = true,
                                string language = "Română")
            : base(device)
        {
            _subtitlesEnabled = subtitles;
            _subtitleLanguage = language;
        }

        public override void Play(string title, string quality)
        {
            Console.WriteLine($"      [VideoPlayer] Pornire film/serial: '{title}'");
            _device.RenderVideo(title, quality, _subtitlesEnabled);
            if (_subtitlesEnabled)
                Console.WriteLine($"      [VideoPlayer] Subtitrări active: {_subtitleLanguage}");
        }

        /// <summary>
        /// Funcționalitate specifică Video — redare episod din serial.
        /// </summary>
        public void PlayEpisode(string series, int season, int episode, string quality)
        {
            string episodeTitle = $"{series} — S{season:D2}E{episode:D2}";
            Console.WriteLine($"      [VideoPlayer] Episod: '{episodeTitle}'");
            _device.RenderVideo(episodeTitle, quality, _subtitlesEnabled);
        }

        public void ToggleSubtitles()
        {
            _subtitlesEnabled = !_subtitlesEnabled;
            Console.WriteLine($"      [VideoPlayer] Subtitrări: {(_subtitlesEnabled ? "ON" : "OFF")}");
        }

        public override string GetPlayerType() => "Video Player (Film / Serial)";
    }
}
