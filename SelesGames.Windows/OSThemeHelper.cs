using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace SelesGames.Phone
{
    public static class OSThemeHelper
    {
        public static OSTheme GetCurrentTheme()
        {
            var backgroundBrush = Application.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;

            if (backgroundBrush == null)
                return OSTheme.Dark;

            if (backgroundBrush.Color == Colors.White)
                return OSTheme.Light;

            else if (backgroundBrush.Color == Colors.Black)
                return OSTheme.Dark;

            else
                return OSTheme.Dark;
        }

        public static Color GetCurrentAccentColor()
        {
            var accentBrush = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            if (accentBrush == null)
                return Colors.Blue;

            return accentBrush.Color;
        }
    }

    public enum OSTheme
    {
        Light,
        Dark
    }
}
