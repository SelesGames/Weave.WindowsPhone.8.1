using Microsoft.Phone.Controls;
using System;
using System.Linq;
using weave;

namespace Weave.Services
{
    public class ArticleListNavigationCorrector : IDisposable
    {
        PhoneApplicationFrame frame;
        PhoneApplicationPage currentPage;

        public ArticleListNavigationCorrector(PhoneApplicationFrame frame)
        {
            this.frame = frame;
            frame.Navigated += OnFrameNavigated;
        }

        void OnFrameNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var lastPage = currentPage;
            currentPage = e.Content as PhoneApplicationPage;

            if (lastPage is MainPage && currentPage is MainPage)
                TryToRemoveLastPageFromBackStack((MainPage)lastPage);
        }

        void TryToRemoveLastPageFromBackStack(MainPage lastPage)
        {
            try
            {
                if (frame.BackStack.Any())
                    frame.RemoveBackEntry();
                lastPage.Dispose();
                lastPage = null;
            }
            catch { }
        }

        public void Dispose()
        {
            frame.Navigated -= OnFrameNavigated;
        }
    }
}
