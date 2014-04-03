using System;
using Windows.UI.Xaml.Data;

namespace SelesGames.Phone.ValueConverters
{
    public class InverseBooleanValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                var val = (bool)value;
                return !val;
            }

            throw new ArgumentException("value");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                var val = (bool)value;
                return !val;
            }

            throw new ArgumentException("value");
        }
    }
}
