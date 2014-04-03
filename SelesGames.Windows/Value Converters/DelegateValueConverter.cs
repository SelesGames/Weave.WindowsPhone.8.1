using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace SelesGames.Phone.ValueConverters
{
    public class DelegateValueConverter : IValueConverter
    {
        Func<object, object> convert, convertBack;

        public DelegateValueConverter(Func<object, object> convert, Func<object, object> convertBack)
        {
            this.convert = convert;
            this.convertBack = convertBack;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (convert != null)
                return convert(value);
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (convertBack != null)
                return convertBack(value);
            else
                return null;
        }
    }

    public class DelegateValueConverter<TInput, TOutput> : IValueConverter
    {
        Func<TInput, TOutput> convert;
        Func<TOutput, TInput> convertBack;

        public DelegateValueConverter(Func<TInput, TOutput> convert, Func<TOutput, TInput> convertBack)
        {
            this.convert = convert;
            this.convertBack = convertBack;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (convert != null && value is TInput)
                return convert((TInput)value);
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (convertBack != null && value is TOutput)
                return convertBack((TOutput)value);
            else
                return null;
        }
    }
}
