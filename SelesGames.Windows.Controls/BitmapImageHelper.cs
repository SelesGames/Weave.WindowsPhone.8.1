using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace SelesGames.Phone.Controls
{
    public class BitmapImageHelper : DependencyObject
    {
        public static readonly DependencyProperty IsBackgroundCreationEnabledProperty = DependencyProperty.RegisterAttached(
            "IsBackgroundCreationEnabled",
            typeof(bool),
            typeof(BitmapImageHelper),
            new PropertyMetadata(IsBackgroundCreationEnabledChanged)
            );

        //[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        public static bool GetIsBackgroundCreationEnabled(DependencyObject source)
        {
            return (bool)source.GetValue(IsBackgroundCreationEnabledProperty);
        }

        //[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        public static void SetIsBackgroundCreationEnabled(DependencyObject source, bool value)
        {
            source.SetValue(IsBackgroundCreationEnabledProperty, value);
        }

        static void IsBackgroundCreationEnabledChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            if (target is BitmapImage && args.NewValue is bool)
                SetForBitmapImage((BitmapImage)target, (bool)args.NewValue);

            else 
                throw new InvalidCastException("You can only use BitmapImageHelper on a BitmapImage");
        }

        static void SetForBitmapImage(BitmapImage bitmapImage, bool newVal)
        {
            if (newVal == false || Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                bitmapImage.CreateOptions = BitmapCreateOptions.DelayCreation;
            }
            else
            {
                bitmapImage.CreateOptions = BitmapCreateOptions.BackgroundCreation;
            }
        }
    }
}
