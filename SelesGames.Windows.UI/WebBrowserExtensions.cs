using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace Microsoft.Phone.Controls
{
    public static class WebBrowserExtensions
    {
        public static IObservable<WebViewNavigationCompletedEventArgs> GetNavigated(this WebView browser)
        {
            return Observable.FromEventPattern<WebViewNavigationCompletedEventArgs>(browser, "NavigationCompleted").Select(o => o.EventArgs);
        }

        public static IObservable<WebViewNavigationStartingEventArgs> GetNavigating(this WebView browser)
        {
            return Observable.FromEventPattern<WebViewNavigationStartingEventArgs>(browser, "NavigationStarting").Select(o => o.EventArgs);
        }

        public static IObservable<WebViewNavigationCompletedEventArgs> GetNavigationFailed(this WebView browser)
        {
            return Observable
                .FromEventPattern<TypedEventHandler<WebView, WebViewNavigationCompletedEventArgs>, WebViewNavigationCompletedEventArgs>(e => browser.NavigationCompleted += e, e => browser.NavigationCompleted -= e)
                .Where(o => !o.EventArgs.IsSuccess)
                .Select(o => o.EventArgs);
        }

        public static IObservable<WebViewDOMContentLoadedEventArgs> GetLoadCompleted(this WebView browser)
        {
            return Observable.FromEventPattern<TypedEventHandler<WebView, WebViewDOMContentLoadedEventArgs>, WebViewDOMContentLoadedEventArgs>(e => browser.DOMContentLoaded += e, e => browser.DOMContentLoaded -= e).Select(o => o.EventArgs);
        }

        public static Task<WebViewNavigationCompletedEventArgs> NavigateToStringAsync(this WebView browser, string html)
        {
            var navigated = browser.GetNavigated().Take(1).ToTask();
            browser.NavigateToString(html);
            return navigated;
        }

        public static Task<WebViewNavigationCompletedEventArgs> NavigateAsync(this WebView browser, Uri uri)
        {
            var navigated = browser.GetNavigated().Take(1).ToTask();
            browser.Navigate(uri);
            return navigated;
        }

        public static Task<WebViewNavigationCompletedEventArgs> NavigateAsync(this WebView browser, Uri uri, byte[] postData, string headers)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            // set request content from postData, etc.
            var navigated = browser.GetNavigated().Take(1).ToTask();
            browser.NavigateWithHttpRequestMessage(request);
            return navigated;
        }
    }

    internal static class IObservableExtensions
    {
        public static Task<T> ToTask<T>(this IObservable<T> observable)
        {
            var hasValue = false;
            var lastValue = default(T);
            var disposable = new System.Reactive.Disposables.SingleAssignmentDisposable();

            var tcs = new TaskCompletionSource<T>();

            try
            {
                disposable.Disposable = observable.Subscribe(
                    value =>
                    {
                        hasValue = true;
                        lastValue = value;
                    },
                    ex =>
                    {
                        tcs.TrySetException(ex);
                        disposable.Dispose();
                    },
                    () =>
                    {
                        if (hasValue)
                            tcs.TrySetResult(lastValue);
                        else
                            tcs.TrySetException(
                                new InvalidOperationException("no elements in this observable sequence - IObservableExtensions.ToTask"));
                        disposable.Dispose();
                    });
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }
    }
}
