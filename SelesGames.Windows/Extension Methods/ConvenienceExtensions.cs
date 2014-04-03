using Windows.UI.Xaml;

public static class ConvenienceExtensions
{
    public static bool IsInDesignMode(this UIElement element)
    {
        return Windows.ApplicationModel.DesignMode.DesignModeEnabled;
    }
}