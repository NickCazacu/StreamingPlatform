namespace StreamingPlatform.Interfaces
{
    public interface IButton
    {
        string Render();
        string OnClick(string action);
    }

    public interface IMenu
    {
        string Render();
        string SelectItem(string item);
    }

    public interface IMediaPlayer
    {
        string Render();
        string Play(string contentTitle);
        string Pause();
        string ShowControls();
    }

    public interface IDialog
    {
        string Show(string title, string message);
        string Close();
    }

    public interface IUIFactory
    {
        IButton CreateButton(string label);
        IMenu CreateMenu(string title);
        IMediaPlayer CreateMediaPlayer();
        IDialog CreateDialog();
        string GetPlatformName();
    }
}
