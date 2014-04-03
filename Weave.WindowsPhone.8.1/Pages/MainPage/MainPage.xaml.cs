using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SelesGames;
using SelesGames.Phone;
using SelesGames.UI.Advertising;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Weave.Customizability;
using Weave.SavedState;
using Weave.ViewModels;
using Weave.ViewModels.Contracts.Client;
using Portable.Common;
using System.Net;
using System.Windows.Navigation;
using Weave.WP.ViewModels.MainPage;
using Weave.WP.ViewModels;
using Weave.WP.ViewModels.GroupedNews;
using Weave.Services;


namespace weave
{
    public partial class MainPage : PhoneApplicationPage, IDisposable
    {
        bool isPageInitialized = false;
        string header = null;
        string mode = null;
        Guid? feedId = null;
        Uri currentUri = null;

        PermanentState permState;
        UserInfo user;

        MainPageViewModel vm;
        FeedsToNewsItemGroupAdapter feedsListenerVM;
        SwitchingAdControl adControl;

        ApplicationBarIconButton refreshButton, fontButton, markPageReadButton, manageSourceButton, addSourceButton, searchSourceButton;
        ApplicationBarMenuItem lockOrientationButton, openNavMenuButton, pinToStartScreenButton; 
        CompositeDisposable pageLevelDisposables = new CompositeDisposable();
        SwipeGestureHelper swipeHelper;
        MenuMode currentMenuMode = MenuMode.Hidden;
        IApplicationBar mainAppBar, sourcesListAppBar;

        SelesGames.PopupService<Unit> fontSizePopupService;
        FontAndThemePopup fontSizePopup;
        Brush transparentBrush;




        #region Constructor

        public MainPage()
        {
            InitializeComponent();
            transparentBrush = ContentGrid.Background;

            SourcesList.Visibility = Visibility.Visible;
            MinTitlePanel.RenderTransform = new CompositeTransform();
            ContentGrid.RenderTransform = new CompositeTransform();
            cl.RenderTransform = new CompositeTransform();
            SourcesList.RenderTransform = new CompositeTransform();

            if (DesignerProperties.IsInDesignTool)
                return;

            this.Loaded += OnLoaded;
            SetValue(RadTransitionControl.TransitionProperty, new RadContinuumAndSlideTransition());

            permState = ServiceResolver.Get<PermanentState>();
            var isAppBarMinimized = permState.IsHideAppBarOnArticleListPageEnabled;
            ApplicationBar.Mode = isAppBarMinimized ? ApplicationBarMode.Minimized : ApplicationBarMode.Default;
            bottomBarFill.Height = isAppBarMinimized ? 30d : 72d;

            CreateMainAppBar();
            CreateSourcesListAppBar();
            BindIsOrientationLockedToAppBar();

            fontSizePopup = ServiceResolver.Get<FontAndThemePopup>();
            Observable.FromEventPattern<SelesGames.EventArgs<ArticleListFormatProperties>>(fontSizePopup, "ArticleListFormatChanged")
                .Subscribe(o => OnArticleListFormatChanged(fontSizePopup, o.EventArgs)).DisposeWith(pageLevelDisposables);

            user = ServiceResolver.Get<UserInfo>();
        }

        void ApplyThemeToControl()
        {
            var theme = permState.ArticleListFormat;

            if (theme == ArticleListFormatType.Card)
            {
                ContentGrid.Background = new SolidColorBrush(Color.FromArgb(255, 237, 237, 237));
            }
            else
            {
                ContentGrid.Background = transparentBrush;
            }
            cl.ArticleTheme = theme;
        }

        void OnArticleListFormatChanged(object sender, SelesGames.EventArgs<ArticleListFormatProperties> eventArgs)
        {
            ApplyThemeToControl();
        }

        void CreateMainAppBar()
        {
            mainAppBar = ApplicationBar;
            markPageReadButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            refreshButton = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
            fontButton = ApplicationBar.Buttons[2] as ApplicationBarIconButton;
            lockOrientationButton = ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
            pinToStartScreenButton = ApplicationBar.MenuItems[1] as ApplicationBarMenuItem;
            openNavMenuButton = ApplicationBar.MenuItems[2] as ApplicationBarMenuItem;
        }

        void CreateSourcesListAppBar()
        {
            sourcesListAppBar = new ApplicationBar
            {
                BackgroundColor = mainAppBar.BackgroundColor,
                ForegroundColor = mainAppBar.ForegroundColor,
                Mode = ApplicationBarMode.Default,
                Opacity = mainAppBar.Opacity,
                IsMenuEnabled = mainAppBar.IsMenuEnabled,
                IsVisible = mainAppBar.IsVisible,
            };

            manageSourceButton = new ApplicationBarIconButton(new Uri("/Assets/Icons/appbar.rss.png", UriKind.Relative)) { Text = "manage" };
            addSourceButton = new ApplicationBarIconButton(new Uri("/Assets/Icons/appbar.add.rest.png", UriKind.Relative)) { Text = "add" };
            searchSourceButton = new ApplicationBarIconButton(new Uri("/Assets/Icons/appbar.feature.search.rest.png", UriKind.Relative)) { Text = "search" };

            manageSourceButton.GetClick().Subscribe(o => OnManageSourceButtonClick()).DisposeWith(this.pageLevelDisposables);
            addSourceButton.GetClick().Subscribe(o => OnAddSourceButtonClick()).DisposeWith(this.pageLevelDisposables);
            searchSourceButton.GetClick().Subscribe(o => OnSearchSourceClick()).DisposeWith(this.pageLevelDisposables);

            sourcesListAppBar.Buttons.Add(manageSourceButton);
            sourcesListAppBar.Buttons.Add(addSourceButton);
            sourcesListAppBar.Buttons.Add(searchSourceButton);
        }

        void OnManageSourceButtonClick()
        {
            NavigationService.ToManageSourcesPage();
        }
        void OnSearchSourceClick()
        {
            NavigationService.ToAddSourcePage("search");
        }
        void OnAddSourceButtonClick()
        {
            NavigationService.ToAddSourcePage("browse");
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.OrientationChanged += OnOrientationChanged;
            this.Unloaded += OnUnloaded;
            ChangeBrowserMarginsByPageLayout();
        }

        // Remove event handlers on page unloaded
        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.OrientationChanged -= OnOrientationChanged;
            this.Unloaded -= OnUnloaded;
        }

        // change the margins on the webBrowser on page orientation changed
        void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            ChangeBrowserMarginsByPageLayout();
        }

        void ChangeBrowserMarginsByPageLayout()
        {
            //DebugEx.WriteLine("orientation: {0}", Orientation.ToString());
            if (Orientation.IsAnyPortrait())
            {
                bottomBarFill.Visibility = Visibility.Visible;
                leftBarFill.Visibility = Visibility.Collapsed;
                rightBarFill.Visibility = Visibility.Collapsed;
            }
            else if (Orientation == PageOrientation.LandscapeLeft)
            {
                rightBarFill.Visibility = Visibility.Visible;
                leftBarFill.Visibility = Visibility.Collapsed;
                bottomBarFill.Visibility = Visibility.Collapsed;
            }
            else if (Orientation == PageOrientation.LandscapeRight)
            {
                leftBarFill.Visibility = Visibility.Visible;
                rightBarFill.Visibility = Visibility.Collapsed;
                bottomBarFill.Visibility = Visibility.Collapsed;
            }
        }

        void BindIsOrientationLockedToAppBar()
        {
            var orientationLockService = ServiceResolver.Get<OrientationLockService>();

            var bindingAdapter = new ApplicationBarToggleMenuItemAdapter(lockOrientationButton)
            {
                CheckedText = "turn off \"soft\" rotation lock",
                UncheckedText = "turn on \"soft\" rotation lock",
                IsChecked = orientationLockService.IsLocked,
            };
            bindingAdapter.SetBinding(ApplicationBarToggleMenuItemAdapter.IsCheckedProperty,
                 new Binding("IsLocked") { Source = orientationLockService, Mode = BindingMode.TwoWay });
            bindingAdapter.DisposeWith(pageLevelDisposables);
        }

        #endregion




        #region OnNavigatedTo (overloaded)

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                if (isPageInitialized)
                {
                    ZoomInSB.Stop();
                    if (vm != null)
                    {
                        await this.GetLoaded().Take(1).ToTask();
                        await vm.OnNavigatedTo(e.NavigationMode);
                    }
                }
                else
                {
                    isPageInitialized = true;
                    FinishPageInitialization();

                    feedsListenerVM = ServiceResolver.Get<FeedsToNewsItemGroupAdapter>();
                    SourcesList.DataContext = feedsListenerVM;

                    await OnNavigatedTo(e.Uri, e.NavigationMode);
                }
            }
            catch (Exception exception)
            {
                DebugEx.WriteLine("exception in MainPage onnavto, {0}", exception);
                NavigationService.TryGoBack();
            }
        }

        public async Task OnNavigatedTo(Uri navUri, NavigationMode navigationMode)
        {
            string header = null;
            string mode = null;
            Guid? feedId = null;

            currentUri = navUri;

            pinToStartScreenButton.IsEnabled = IsPinToStartButtonEnabled();

            var lookup = currentUri.ParseQueryString().ToDictionary(o => o.Key, o => o.Value);

            if (lookup.ContainsKey("header"))
            {
                header = HttpUtility.UrlDecode(lookup["header"]);
            }
            if (lookup.ContainsKey("mode"))
            {
                mode = lookup["mode"];
            }
            if (lookup.ContainsKey("feedId"))
            {
                mode = "feed";
                feedId = Guid.Parse(lookup["feedId"]);
            }

            if (mode == "sources")
            {
                ShowMenuNoAnimation();
                return;
            }

            if (mode == this.mode && header == this.header)
                HideMenuNoAnimation();
            else
            {
                this.header = header;
                this.mode = mode;
                this.feedId = feedId;

                cl.ItemsSource = null;

                CreateViewModel();
                HideMenuNoAnimation();

                if (vm != null)
                    await vm.OnNavigatedTo();

                if (feedsListenerVM != null)
                    feedsListenerVM.UpdateTileData();
            }
        }

        #endregion




        #region Create the VIEWMODEL

        void CreateViewModel()
        {
            if (this.vm != null)
            {
                this.vm.Dispose();
            }

            if (string.IsNullOrWhiteSpace(header))
            {
                DataContext = null;
                this.vm = null;
                return;
            }

            var tallyer = permState.RunHistory.GetActiveLog();

            NewsItemGroup group = null;
            if (mode.Equals("category", StringComparison.OrdinalIgnoreCase))
            {
                group = feedsListenerVM.Find(header);
                tbPageCount.Visibility = Visibility.Visible;
                tallyer.Tally(header);
            }
            else if (mode.Equals("feed", StringComparison.OrdinalIgnoreCase))
            {
                group = feedsListenerVM.Find(feedId.Value);
                tbPageCount.Visibility = Visibility.Visible;
                tallyer.Tally(header);
            }
            else if (mode.Equals("favorites", StringComparison.OrdinalIgnoreCase))
            {
                group = new FavoriteArticlesGroup(user);
                tbPageCount.Visibility = Visibility.Collapsed;
            }
            else if (mode.Equals("read", StringComparison.OrdinalIgnoreCase))
            {
                group = new ReadArticlesGroup(user);
                tbPageCount.Visibility = Visibility.Collapsed;
            }

            this.vm = new MainPageViewModel(this, header, group);
            DataContext = this.vm;

            vm.RecoverSavedTombstoneState();
        }

        #endregion




        #region DELAYED PAGE INTIALIZATION - performed on initial page navigation.  This delayed form of initialization allows for faster intial page draw when navigating to this page

        void FinishPageInitialization()
        {
            InitializeAdControl();
            InitializeCustomListAndImageCache();
            InitializeTouchSwipingHandling();
            InitializeApplicationBarButtonEventHandlers();
            InitializeNavMenuHandlers();
        }

        async void InitializeAdControl()
        {
            var adService = ServiceResolver.Get<AdService>();

            await adService.Initialization;

            if (!adService.ShouldDisplayAds)
                return;

            adControl = adService.CreateAdControl(this.header);
            //adControl.AdMargin = new Thickness(0);
            //adControl.AdHeight = 80;
            //adControl.AdWidth = 480;
            adControl.SetValue(Grid.RowProperty, 0);
            adControl.SetValue(Grid.ColumnProperty, 1);
            adControl.PlayAnimations = true;
            adControl.Opacity = 0;

            LayoutRoot.Children.Add(adControl);
            await TimeSpan.FromSeconds(1);
            adControl.Fade().From(0).To(1).Over(TimeSpan.FromSeconds(0.7)).ToStoryboard().Begin();
        }
        
        void InitializeCustomListAndImageCache()
        {
            ApplyThemeToControl();
            SubscribeToNewsItemClicked();
        }

        void InitializeTouchSwipingHandling()
        {
            swipeHelper = new SwipeGestureHelper(LayoutRoot);
            swipeHelper.Swipe += OnSwipe;
            swipeHelper.DisposeWith(pageLevelDisposables);
        }

        void OnSwipe(object sender, SwipeGestureHelper.SwipeEventArgs e)
        {
            if (e.Direction == SwipeGestureHelper.SwipeDirection.Left)
                OnPreviousPage();
            else if (e.Direction == SwipeGestureHelper.SwipeDirection.Right)
                OnNextPage();
        }

        /// <summary>
        /// Initialize all button handlers on page (refresh, next/previous page, font, mark page read)
        /// </summary>
        void InitializeApplicationBarButtonEventHandlers()
        {
            refreshButton.GetClick().Where(_ => vm != null)
                .Subscribe(() => vm.ManualRefresh().Fire(OnManualRefreshException))
                .DisposeWith(pageLevelDisposables);

            fontButton.GetClick().Subscribe(LaunchLocalSettingsPopup).DisposeWith(pageLevelDisposables);
            markPageReadButton.GetClick().Subscribe(OnAllRead).DisposeWith(pageLevelDisposables);
            pinToStartScreenButton.GetClick().Subscribe(OnPinToStartButtonPressed).DisposeWith(pageLevelDisposables);
            openNavMenuButton.GetClick().Subscribe(ShowMenu).DisposeWith(pageLevelDisposables);
        }

        void OnManualRefreshException(Exception e)
        {
            MessageBox.Show("Error refreshing - try going back to Weave's panoramic homepage, then coming back into this category/feed");
        }

        void LaunchLocalSettingsPopup()
        {
            if (PopupService.IsOpen)
                return;

            fontSizePopupService = new SelesGames.PopupService<System.Reactive.Unit>(fontSizePopup)
            {
                CloseOnNavigation = false,
            };
            fontSizePopupService.BeginShow();
        }

        void InitializeNavMenuHandlers()
        {
            SourcesList.ItemSelected += SourcesList_ItemSelected;
        }

        void SourcesList_ItemSelected(object sender, CategoryOrFeedEventArgs e)
        {
            var niGroup = e.Selected;
            if (niGroup == null)
                return;

            if (niGroup is AllNewsGroup || niGroup is CategoryGroup)
            {
                if (this.mode == "category" && header.Equals(niGroup.DisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    HideMenu();
                    return;
                }
            }
            else if (niGroup is FeedGroup)
            {
                var feedGroup = (FeedGroup)niGroup;
                if (feedGroup.Feed.Id.Equals(this.feedId))
                {
                    HideMenu();
                    return;
                }
            }

            niGroup.MarkEntry();
            GlobalNavigationService.ToMainPage(niGroup);
        }

        #endregion




        #region Sources/Categories list show/hide menu

        void ShowMenuNoAnimation()
        {
            currentMenuMode = MenuMode.Displayed;
            MinTitlePanel.SoftCollapse();
            ContentGrid.SoftCollapse();
            SourcesList.SoftMakeVisible();
            ApplicationBar = sourcesListAppBar;
        }

        void ShowMenu()
        {
            if (currentMenuMode == MenuMode.Displayed)
                return;

            currentMenuMode = MenuMode.Displayed;
            MinTitlePanel.IsHitTestVisible = false;
            ContentGrid.IsHitTestVisible = false;
            SourcesList.SoftMakeVisible();
            HideCategoriesListSB.Stop();
            ShowCategoriesListSB.Begin();
            ApplicationBar = sourcesListAppBar;
        }

        void HideMenuNoAnimation()
        {
            currentMenuMode = MenuMode.Hidden;
            SourcesList.SoftCollapse();
            MinTitlePanel.SoftMakeVisible();
            ContentGrid.SoftMakeVisible();
            ApplicationBar = mainAppBar;
        }

        void HideMenu()
        {
            if (currentMenuMode == MenuMode.Hidden)
                return;

            currentMenuMode = MenuMode.Hidden;
            SourcesList.IsHitTestVisible = false;
            MinTitlePanel.IsHitTestVisible = true;
            ContentGrid.IsHitTestVisible = true;
            ShowCategoriesListSB.Stop();
            HideCategoriesListSB.Begin();
            ApplicationBar = mainAppBar;
        }

        #endregion




        #region NewsItem click and dismissal handling

        void SubscribeToNewsItemClicked()
        {
            cl.NewsItemSelected
                .Subscribe(OnNewsItemClicked)
                .DisposeWith(pageLevelDisposables);
        }

        async void OnNewsItemClicked(System.Tuple<object, NewsItem> tup)
        {
            var newsItem = (NewsItem)tup.Item2;
            IsHitTestVisible = false;
            ZoomInSB.Begin();
            await Task.Delay(TimeSpan.FromSeconds(0.18d));
            GlobalNavigationService.ToWebBrowserPage(newsItem);
            IsHitTestVisible = true;
        }

        #endregion




        #region Mark All Read

        void OnAllRead()
        {
            if (vm != null)
                vm.MarkCurrentPageRead();
        }

        #endregion




        #region Pin to Start

        bool IsPinToStartButtonEnabled()
        {
            return !ShellTile.ActiveTiles.Any(x => x.NavigationUri.Equals(currentUri));
        }

        async void OnPinToStartButtonPressed()
        {
            if (vm == null)
                return;

            try
            {
                var currentSource = currentUri;

                // Look to see if the tile already exists and if so, don't try to create again.
                ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.Equals(currentSource));

                // Create the tile if we didn't find it already exists.
                if (TileToFind == null)
                {
                    var liveTileVM = await vm.CreateLiveTileViewModel();
                    var newTileData = liveTileVM.CreateTileData();

                    // Create the tile and pin it to Start. This will cause a navigation to Start and a deactivation of our application.
                    ShellTile.Create(currentSource, newTileData, true);
                }
            }
            catch (InvalidOperationException ex)
            {
                DebugEx.WriteLine(ex);
                if (ex.Message == "image count")
                    MessageBox.Show("We can only make a Live Tile out of a news source that has at least 2 images in it!");
                else
                    MessageBox.Show("Whoops!  There was a problem creating the Live Tile for this category.  Please try again later");
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
                MessageBox.Show("Whoops!  There was a problem creating the Live Tile for this category.  Please try again later");
            }
        }

        #endregion




        #region Page Change logic

        void OnPreviousPage()
        {
            if (vm != null && vm.HasPrevious)
            {
                PreparePageChangeAnimation(PageChangeAnimateDirection.Previous);
                vm.CurrentPage--;
            }
            else if (currentMenuMode == MenuMode.Hidden)
            {
                ShowMenu();
            }
        }

        void OnNextPage()
        {
            if (vm != null && currentMenuMode == MenuMode.Displayed)
            {
                HideMenu();
            }
            else if (vm != null && vm.HasNext)
            {
                PreparePageChangeAnimation(PageChangeAnimateDirection.Next);
                vm.CurrentPage++;
            }
        }

        enum PageChangeAnimateDirection
        {
            Previous,
            Next
        }

        enum MenuMode
        {
            Displayed,
            Hidden
        }




        #region Page Change animation helpers

        bool isInPageChangeAnimation = false;

        void PreparePageChangeAnimation(PageChangeAnimateDirection direction)
        {
            if (isInPageChangeAnimation)
                return;

            isInPageChangeAnimation = true;

            if (direction == PageChangeAnimateDirection.Previous)
                previousPageStartSB.Begin();

            else if (direction == PageChangeAnimateDirection.Next)
                nextPageStartSB.Begin();
        }

        internal void CompletePageChangeAnimation(List<NewsItem> source, int direction = 0)
        {
            isInPageChangeAnimation = false;
            this.nextPageStartSB.Stop();
            this.previousPageStartSB.Stop();

            cl.AnimationDirection = direction;
            cl.ItemsSource = source;
            cl.Visibility = Visibility.Visible;
        }

        #endregion




        #endregion




        #region Navigation From, BackKeyPress

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (vm != null)
                vm.SaveTransientState();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (PopupService.IsOpen)
                return;

            IsHitTestVisible = false;
            base.OnBackKeyPress(e);
        }

        #endregion




        #region Show/Hide Radial ProgressBar

        internal void ShowRadialProgressBar()
        {
            Dispatcher.BeginInvoke(() =>
            {
                loadingIndicator.IsPlaying = true;
                loadingIndicator.Visibility = Visibility.Visible;
            });
        }

        internal void HideRadialProgressBar()
        {
            Dispatcher.BeginInvoke(() =>
            {
                loadingIndicator.IsPlaying = false;
                loadingIndicator.Visibility = Visibility.Collapsed;
            });
        }

        #endregion




        #region Finalizer and IDisposable

        ~MainPage()
        {
            DebugEx.WriteLine("MainPage {0} was finalized", header);
        }

        public void Dispose()
        {
            this.pageLevelDisposables.Dispose();

            using (this.cl)
            using (this.vm)
            using (this.adControl)
            { }

            if (swipeHelper != null)
                swipeHelper.Swipe -= OnSwipe;
        }

        #endregion
    }
}