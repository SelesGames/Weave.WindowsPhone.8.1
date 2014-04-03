using Microsoft.Phone.Controls;
using System;
using Windows.UI.Xaml.Controls;

namespace SelesGames.Phone
{
    internal class PhoneApplicationPageExtendedInfo
    {
        WeakReference page;
        internal Page Page
        {
            get { return page.Target as Page; }
        }
        internal SupportedPageOrientation OriginalSupportedOrientation { get; private set; }

        internal static PhoneApplicationPageExtendedInfo Create(Page page)
        {
            var o = new PhoneApplicationPageExtendedInfo
            {
                OriginalSupportedOrientation = page.SupportedOrientations
            };
            o.page = new WeakReference(page);
            return o;
        }

        internal void SetSupportedOrientation(SupportedPageOrientation orientation)
        {
            if (page.Target is Page)
                ((Page)page.Target).SupportedOrientations = orientation;
        }

        internal void RestoreOriginalSupportedOrientation()
        {
            if (page.Target is Page)
                ((Page)page.Target).SupportedOrientations = OriginalSupportedOrientation;
        }

        internal PageOrientation GetPageOrientation()
        {
            if (page.Target is Page)
                return ((Page)page.Target).Orientation;
            else
                return PageOrientation.None;
        }
    }
}
