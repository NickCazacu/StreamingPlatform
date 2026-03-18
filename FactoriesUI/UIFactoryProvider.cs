using System;
using StreamingPlatform.Interfaces;
using StreamingPlatform.Models;

namespace StreamingPlatform.Factories.UI
{
    public static class UIFactoryProvider
    {
        public static IUIFactory GetFactory(PlatformTheme platform)
        {
            return platform switch
            {
                PlatformTheme.Windows => new WindowsUIFactory(),
                PlatformTheme.Mac => new MacUIFactory(),
                PlatformTheme.Linux => new LinuxUIFactory(),
                _ => throw new ArgumentException($"Platformă necunoscută: {platform}")
            };
        }
    }

    public class StreamingApp
    {
        private readonly IUIFactory _uiFactory;
        private readonly IButton _playButton;
        private readonly IButton _pauseButton;
        private readonly IMenu _mainMenu;
        private readonly IMediaPlayer _player;
        private readonly IDialog _dialog;

        public StreamingApp(IUIFactory factory)
        {
            _uiFactory = factory;

            _playButton = factory.CreateButton("Play");
            _pauseButton = factory.CreateButton("Pause");
            _mainMenu = factory.CreateMenu("Streaming Platform");
            _player = factory.CreateMediaPlayer();
            _dialog = factory.CreateDialog();
        }

        public string RenderUI()
        {
            return $"=== Interfața pentru {_uiFactory.GetPlatformName()} ===\n" +
                   $"  Menu: {_mainMenu.Render()}\n" +
                   $"  Player: {_player.Render()}\n" +
                   $"  {_playButton.Render()}\n" +
                   $"  {_pauseButton.Render()}";
        }

        public string PlayContent(string title)
        {
            var result = _player.Play(title);
            result += "\n  " + _player.ShowControls();
            return result;
        }

        public string PauseContent()
        {
            return _player.Pause();
        }

        public string ShowNotification(string title, string message)
        {
            return _dialog.Show(title, message);
        }

        public string NavigateMenu(string item)
        {
            return _mainMenu.SelectItem(item);
        }

        public string ClickButton(string buttonAction)
        {
            return _playButton.OnClick(buttonAction);
        }
    }
}
