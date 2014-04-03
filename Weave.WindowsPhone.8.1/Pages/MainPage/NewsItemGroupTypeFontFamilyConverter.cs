using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Weave.WP.ViewModels.GroupedNews;

namespace weave
{
    public class NewsItemGroupTypeFontFamilyConverter : IValueConverter
    {
        #region static media values that come from App.Resources

        static bool areFontFamiliesSet = false;
        static FontFamily categoryFont, feedFont;

        static void SetFontFamilies()
        {
            categoryFont = Application.Current.Resources["PhoneFontFamilyBlack"] as FontFamily;
            feedFont = Application.Current.Resources["PhoneFontFamilyNormal"] as FontFamily;
            areFontFamiliesSet = true;
        }

        #endregion




        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!areFontFamiliesSet)
                SetFontFamilies();

            if (value is CategoryGroup || value is AllNewsGroup)
                return categoryFont;

            else if (value is FeedGroup)
                return feedFont;

            return categoryFont;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
