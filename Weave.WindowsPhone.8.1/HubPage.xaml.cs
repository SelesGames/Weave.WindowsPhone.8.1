using Weave.WindowsPhone._8._1.Common;
using Weave.WindowsPhone._8._1.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Weave.WindowsPhone._8._1
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public HubPage()
        {
            this.InitializeComponent();

            //Hub is only supported in Portrait orientation
            DisplayProperties.AutoRotationPreferences = DisplayOrientations.Portrait;

            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;

            if (this.BottomAppBar != null)
                this.BottomAppBar.SizeChanged += AppBar_SizeChanged;

            var settings = new AccessibilitySettings();
            //Turn the background off for high contrast
            if(settings.HighContrast)
            {
                Hub.Background = null;
            }
            settings.HighContrastChanged += settings_HighContrastChanged;
        }

        void settings_HighContrastChanged(AccessibilitySettings sender, object args)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (sender.HighContrast)
                {
                    Hub.Background = null;
                }
                else
                {
                    var backgroundBrush = new ImageBrush();
                    var image = new BitmapImage(new Uri("ms-appx:///Assets/HubBackground.png", UriKind.RelativeOrAbsolute));
                    backgroundBrush.ImageSource = image;
                    Hub.Background = backgroundBrush;
                }
            });
        }

        /// <summary>
        /// Check app bar size changed event and reserve space for app bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AppBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Reserve space on the page when an opaque AppBar.
            bool reserveAppBarSpace = this.BottomAppBar.Opacity >= 1.0
                && this.BottomAppBar.Visibility == Visibility.Visible;

            if (reserveAppBarSpace)
            {
                Thickness defaultPageMargin = default(Thickness);
                switch (DisplayProperties.CurrentOrientation)
                {
                    case DisplayOrientations.Landscape:
                        defaultPageMargin.Right = this.BottomAppBar.Width;
                        break;
                    case DisplayOrientations.LandscapeFlipped:
                        defaultPageMargin.Left = this.BottomAppBar.Width;
                        break;
                    case DisplayOrientations.Portrait:
                        defaultPageMargin.Bottom = this.BottomAppBar.Height;
                        break;
                }

                this.Margin = defaultPageMargin;
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data

            var sampleDataGroups = await SampleDataSource.GetGroupsAsync();
            this.DefaultViewModel["SectionGroups"] = sampleDataGroups;

            var sampleDataGroup2 = await SampleDataSource.GetGroupAsync("Group-2");
            this.DefaultViewModel["Section2Items"] = sampleDataGroup2;

            var sampleDataGroup3 = await SampleDataSource.GetGroupAsync("Group-3");
            this.DefaultViewModel["Section3Items"] = sampleDataGroup3;

            var sampleDataGroup4 = await SampleDataSource.GetGroupAsync("Group-4");
            this.DefaultViewModel["Section4Items"] = sampleDataGroup4;

            var sampleDataGroup5 = await SampleDataSource.GetGroupAsync("Group-5");
            this.DefaultViewModel["Section5Items"] = sampleDataGroup5;


        }
        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>
        /// <param name="sender">The GridView or ListView
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(ItemPage), itemId);
        }
        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
