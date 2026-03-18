using StreamingPlatform.Interfaces;

namespace StreamingPlatform.Factories.UI
{
    public class MacButton : IButton
    {
        private readonly string _label;
        public MacButton(string label) { _label = label; }

        public string Render()
            => $"[Mac Button] (  {_label}  ) - Aqua Style, rounded corners";
        public string OnClick(string action)
            => $"[Mac] Butonul '{_label}' a executat: {action}";
    }

    public class MacMenu : IMenu
    {
        private readonly string _title;
        public MacMenu(string title) { _title = title; }

        public string Render()
            => $"[Mac Menu]  {_title}  File  Edit  View  (Top Menu Bar Style)";
        public string SelectItem(string item)
            => $"[Mac] Element selectat din meniu: {item}";
    }

    public class MacMediaPlayer : IMediaPlayer
    {
        public string Render()
            => "[Mac QuickTime Player] ╭──────────────────╮ │  QuickTime Style │ ╰──────────────────╯";
        public string Play(string contentTitle)
            => $"[Mac] Se redă în QuickTime: {contentTitle}";
        public string Pause()
            => "[Mac] QuickTime: Pauză";
        public string ShowControls()
            => "[Mac] Controale: ◁  ❚❚  ▷  )) ──●────── (macOS Style)";
    }

    public class MacDialog : IDialog
    {
        public string Show(string title, string message)
            => $"[Mac Dialog] ╭── {title} ──╮\n              │ {message} │\n" +
               $"              │  (Cancel)  (OK)  │\n              ╰─────────────────╯";
        public string Close()
            => "[Mac] Dialog închis cu click în afara ferestrei";
    }

    public class MacUIFactory : IUIFactory
    {
        public IButton CreateButton(string label) => new MacButton(label);
        public IMenu CreateMenu(string title) => new MacMenu(title);
        public IMediaPlayer CreateMediaPlayer() => new MacMediaPlayer();
        public IDialog CreateDialog() => new MacDialog();
        public string GetPlatformName() => "macOS Sonoma";
    }
}
