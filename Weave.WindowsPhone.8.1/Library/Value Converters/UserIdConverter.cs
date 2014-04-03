using System;
using Windows.UI.Xaml.Data;

namespace weave
{
    public class UserIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Guid)
            {
                return string.Format("id: {0}", ((Guid)value).ToString("N"));
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
