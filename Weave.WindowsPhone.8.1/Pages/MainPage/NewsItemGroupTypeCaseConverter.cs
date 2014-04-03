using System;
using System.Windows.Data;
using Weave.WP.ViewModels.GroupedNews;

namespace weave
{
    public class NewsItemGroupTypeCaseConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is NewsItemGroup))
                return null;

            var val = (NewsItemGroup)value;

            if (string.IsNullOrWhiteSpace(val.DisplayName))
                return null;

            if (value is CategoryGroup || value is AllNewsGroup)
                return val.DisplayNameUppercase;

            else if (value is FeedGroup)
                return val.DisplayName;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
