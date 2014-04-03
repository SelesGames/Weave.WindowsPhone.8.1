using Microsoft.Phone.Controls;
using System;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Weave.Services
{
    /// <summary>
    /// Class that detects BackNavigation events, disposes the page being navigated away from, and calls Garbage Collection
    /// </summary>
    public class DisposeAndGCCleanupNavHelper : IDisposable
    {
        PhoneApplicationFrame frame;
        PhoneApplicationPage currentPage;
        bool isGCPending = false;

        public DisposeAndGCCleanupNavHelper(PhoneApplicationFrame frame)
        {
            this.frame = frame;
            frame.Navigated += OnFrameNavigated;
        }

        async void OnFrameNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var lastPage = currentPage;
            currentPage = e.Content as PhoneApplicationPage;

            if (e.NavigationMode == NavigationMode.Back)
                using (var disp = lastPage as IDisposable) { }

            if (isGCPending)
                return;

            isGCPending = true;

            await Task.Run(() =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                isGCPending = false;
            });
        }

        public void Dispose()
        {
            frame.Navigated -= OnFrameNavigated;
        }
    }
}
