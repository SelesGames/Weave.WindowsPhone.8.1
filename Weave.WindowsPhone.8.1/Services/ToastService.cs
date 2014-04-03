using System.Windows;
using ToastPromptControl;

namespace Weave.Services
{
    public static class ToastService
    {
        public static void ToastPrompt(string message)
        {
            new ToastPrompt
            {
                MillisecondsUntilHidden = 2000,
                Message = message,
                FontSize = (double)Application.Current.Resources["PhoneFontSizeMediumLarge"],
                //TextOrientation = System.Windows.Controls.Orientation.Vertical
            }
            .Show();
        }
    }
}
