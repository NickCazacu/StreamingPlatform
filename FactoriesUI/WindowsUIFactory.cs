using StreamingPlatform.Interfaces;

namespace StreamingPlatform.Factories.UI
{
    public class WindowsButton : IButton
    {
        private readonly string _label;
        public WindowsButton(string label) { _label = label; }

        public string Render()
            => $"[Windows Button] ┌─────────────┐ │  {_label}  │ └─────────────┘";
        public string OnClick(string action)
            => $"[Windows] Butonul '{_label}' a executat: {action}";
    }

    public class WindowsMenu : IMenu
    {
        private readonly string _title;
        public WindowsMenu(string title) { _title = title; }

        public string Render()
            => $"[Windows Menu] ═══ {_title} ═══ (Ribbon Style)";
        public string SelectItem(string item)
            => $"[Windows] Element selectat din meniu: {item}";
    }

    public class WindowsMediaPlayer : IMediaPlayer
    {
        public string Render()
            => "[Windows Media Player] ╔══════════════════╗ ║  WMP Style UI    ║ ╚══════════════════╝";
        public string Play(string contentTitle)
            => $"[Windows] Se redă în WMP: {contentTitle}";
        public string Pause()
            => "[Windows] WMP: Pauză";
        public string ShowControls()
            => "[Windows] Controale: [⏮] [⏸] [⏭] [🔊] ▬▬▬●▬▬ (Windows Style)";
    }

    public class WindowsDialog : IDialog
    {
        public string Show(string title, string message)
            => $"[Windows Dialog] ╔═ {title} ═╗\n                  ║ {message} ║\n" +
               $"                  ║  [OK]  [Cancel]  ║\n                  ╚══════════════════╝";
        public string Close()
            => "[Windows] Dialog închis cu [X]";
    }

    public class WindowsUIFactory : IUIFactory
    {
        public IButton CreateButton(string label) => new WindowsButton(label);
        public IMenu CreateMenu(string title) => new WindowsMenu(title);
        public IMediaPlayer CreateMediaPlayer() => new WindowsMediaPlayer();
        public IDialog CreateDialog() => new WindowsDialog();
        public string GetPlatformName() => "Windows 11";
    }
}
