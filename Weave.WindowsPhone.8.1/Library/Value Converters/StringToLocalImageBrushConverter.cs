using System;
using System.Net;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace weave
{
    public class StringToLocalImageBrushConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string source = (string)value;
                var uri = source.ToUri(UriKind.Relative);
                if (uri == null)
                    return null;

                BitmapImage image = new BitmapImage(uri);
                if (image == null)
                    return null;

                var brush = new ImageBrush { ImageSource = image };
                return brush;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
