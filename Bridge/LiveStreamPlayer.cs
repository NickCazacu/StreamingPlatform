using System;

namespace StreamingPlatform.Bridge
{
    /// <summary>
    /// REFINED ABSTRACTION — Player pentru Live Stream (premiere, concerte, transmisiuni).
    /// Logică specifică: număr privitori în timp real, calitate adaptivă, chat.
    /// </summary>
    public class LiveStreamPlayer : MediaPlayerBase
    {
        private int _currentViewers;
        private bool _isHD;

        public LiveStreamPlayer(IDeviceRenderer device, int initialViewers = 0, bool isHD = true)
            : base(device)
        {
            _currentViewers = initialViewers > 0 ? initialViewers : new Random().Next(500, 50000);
            _isHD           = isHD;
        }

        public override void Play(string title, string quality)
        {
            Console.WriteLine($"      [LivePlayer] Conectare la stream: '{title}' " +
                              $"| {_currentViewers:N0} privitori acum");
            _device.RenderLiveStream(title, _currentViewers, _isHD);
        }

        /// <summary>
        /// Funcționalitate specifică Live — premieră globală cu mai mulți privitori.
        /// </summary>
        public void JoinPremiere(string movieTitle)
        {
            _currentViewers += new Random().Next(5000, 20000);
            Console.WriteLine($"      [LivePlayer] PREMIERĂ: '{movieTitle}' " +
                              $"| {_currentViewers:N0} privitori simultani!");
            _device.RenderLiveStream($"PREMIERĂ: {movieTitle}", _currentViewers, _isHD);
        }

        public void UpdateViewerCount(int delta)
        {
            _currentViewers = Math.Max(0, _currentViewers + delta);
        }

        public override string GetPlayerType() => "Live Stream Player";
    }
}
