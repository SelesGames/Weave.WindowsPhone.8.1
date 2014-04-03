using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    public static class SilverlightToolkitExtensions
    {
        public static IDisposable CorrectForFocusBug(this LongListSelector lls, Control focusTarget)
        {
            return Observable.FromEventPattern<RoutedEventArgs>(lls, "GotFocus").Subscribe(_ => focusTarget.Focus());
        }
    }
}
