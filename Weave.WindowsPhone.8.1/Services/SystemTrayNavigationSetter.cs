using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using weave;
using Weave.SavedState;

namespace Weave.Services
{
    public class SystemTrayNavigationSetter
    {
        PhoneApplicationFrame frame;
        PermanentState permState;

        public SystemTrayNavigationSetter(PhoneApplicationFrame frame, PermanentState permState)
        {
            this.frame = frame;
            this.permState = permState;
            frame.Navigated += OnFrameNavigated;
        }

        void OnFrameNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var currentPage = e.Content as PhoneApplicationPage;
            if (currentPage == null)
                return;

            if (currentPage is SamplePanorama)
                return;
            else
                SystemTray.SetIsVisible(currentPage, permState.IsSystemTrayVisibleWhenPossible);
        }

        public void Dispose()
        {
            frame.Navigated -= OnFrameNavigated;
        }
    }
}
