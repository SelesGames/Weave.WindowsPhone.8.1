using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SelesGames.Phone
{
    public static class PhoneApplicationFrameExtensions
    {
        public static Task<NavigationEventArgs> NavigationStoppedAsync(this Frame frame)
        {
            var tcs = new TaskCompletionSource<NavigationEventArgs>();

            try
            {
                NavigationStoppedEventHandler handler = null;
                handler = (s, e) => 
                {
                    frame.NavigationStopped -= handler;
                    tcs.TrySetResult(e);
                };

                frame.NavigationStopped += handler;
            }

            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }

        public static Task<NavigationEventArgs> NavigatedAsync(this Frame frame)
        {
            var tcs = new TaskCompletionSource<NavigationEventArgs>();

            try
            {
                NavigatedEventHandler handler = null;
                handler = (s, e) =>
                {
                    frame.Navigated -= handler;
                    tcs.TrySetResult(e);
                };

                frame.Navigated += handler;
            }

            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }

        public static Task<NavigatingCancelEventArgs> NavigatingAsync(this Frame frame)
        {
            var tcs = new TaskCompletionSource<NavigatingCancelEventArgs>();

            try
            {
                NavigatingCancelEventHandler handler = null;
                handler = (s, e) =>
                {
                    frame.Navigating -= handler;
                    tcs.TrySetResult(e);
                };

                frame.Navigating += handler;
            }

            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }
    }
}
