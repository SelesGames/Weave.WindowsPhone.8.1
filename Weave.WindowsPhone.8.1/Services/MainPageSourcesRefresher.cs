using Microsoft.Phone.Controls;
using System;
using System.Windows.Navigation;
using weave;
using Weave.ViewModels;

namespace Weave.Services
{
    public class MainPageSourcesRefresher : IDisposable
    {
        PhoneApplicationFrame frame;
        PhoneApplicationPage currentPage, lastPage;
        UserInfo user;

        public MainPageSourcesRefresher(PhoneApplicationFrame frame, UserInfo user)
        {
            this.frame = frame;
            this.user = user;
            frame.Navigated += OnFrameNavigated;
        }

        async void OnFrameNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (!(e.Content is PhoneApplicationPage))
                return;

            lastPage = currentPage;
            currentPage = (PhoneApplicationPage)e.Content;

            if (e.NavigationMode == NavigationMode.Back && 
                currentPage is MainPage &&
                !(lastPage is ReadabilityPage))
            {
                DebugEx.WriteLine("refreshing users feeds");
                try
                {
                    await user.LoadFeeds(refresh: true);
                }
                catch { }
            }

            else if (currentPage is MainPage)
            {
                DebugEx.WriteLine("getting latest feeds for user, no refresh");
                try
                {
                    await user.LoadFeeds(refresh: false);
                }
                catch { }
            }
        }

        public void Dispose()
        {
            frame.Navigated -= OnFrameNavigated;
        }
    }
}
