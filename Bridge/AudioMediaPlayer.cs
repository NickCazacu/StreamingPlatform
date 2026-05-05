using System;

namespace StreamingPlatform.Bridge
{
    /// <summary>
    /// REFINED ABSTRACTION — Player pentru conținut Audio (podcast/coloană sonoră).
    /// Logică specifică: format audio, egalizator, mod background.
    /// </summary>
    public class AudioMediaPlayer : MediaPlayerBase
    {
        private readonly string _audioFormat;
        private readonly bool _backgroundMode;

        public AudioMediaPlayer(IDeviceRenderer device, string format = "AAC",
                                bool backgroundMode = false)
            : base(device)
        {
            _audioFormat    = format;
            _backgroundMode = backgroundMode;
        }

        public override void Play(string title, string quality)
        {
            string mode = _backgroundMode ? " [Mod fundal]" : "";
            Console.WriteLine($"      [AudioPlayer] Pornire audio: '{title}'{mode}");
            _device.RenderAudio(title, _audioFormat);
        }

        /// <summary>
        /// Funcționalitate specifică Audio — redare coloană sonoră a unui film.
        /// </summary>
        public void PlaySoundtrack(string movieTitle)
        {
            Console.WriteLine($"      [AudioPlayer] Coloană sonoră: '{movieTitle}'");
            _device.RenderAudio($"OST: {movieTitle}", _audioFormat);
        }

        public override string GetPlayerType() => $"Audio Player ({_audioFormat})";
    }
}
