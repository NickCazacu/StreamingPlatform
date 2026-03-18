using StreamingPlatform.Interfaces;

namespace StreamingPlatform.Factories.UI
{
    public class LinuxButton : IButton
    {
        private readonly string _label;
        public LinuxButton(string label) { _label = label; }

        public string Render()
            => $"[Linux Button] [ {_label} ] - GTK Theme Style";
        public string OnClick(string action)
            => $"[Linux] Butonul '{_label}' a executat: {action}";
    }

    public class LinuxMenu : IMenu
    {
        private readonly string _title;
        public LinuxMenu(string title) { _title = title; }

        public string Render()
            => $"[Linux Menu] --- {_title} --- (GNOME/KDE Style)";
        public string SelectItem(string item)
            => $"[Linux] Element selectat din meniu: {item}";
    }

    public class LinuxMediaPlayer : IMediaPlayer
    {
        public string Render()
            => "[Linux VLC Player] +------------------+ |  VLC Style UI    | +------------------+";
        public string Play(string contentTitle)
            => $"[Linux] Se redă în VLC: {contentTitle}";
        public string Pause()
            => "[Linux] VLC: Pauză";
        public string ShowControls()
            => "[Linux] Controale: |<  ||  >|  ♪  ===●=== (GTK Style)";
    }

    public class LinuxDialog : IDialog
    {
        public string Show(string title, string message)
            => $"[Linux Dialog] +-- {title} --+\n               | {message} |\n" +
               $"               |  [Cancel]  [OK]  |\n               +------------------+";
        public string Close()
            => "[Linux] Dialog închis cu Alt+F4";
    }

    public class LinuxUIFactory : IUIFactory
    {
        public IButton CreateButton(string label) => new LinuxButton(label);
        public IMenu CreateMenu(string title) => new LinuxMenu(title);
        public IMediaPlayer CreateMediaPlayer() => new LinuxMediaPlayer();
        public IDialog CreateDialog() => new LinuxDialog();
        public string GetPlatformName() => "Linux (Ubuntu/GNOME)";
    }
}
