using Common.Microsoft.OneNote.Response;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SelesGames;
using SelesGames.Phone;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using Weave.Customizability;
using Weave.Microsoft.OneNote;
using Weave.SavedState;
using Weave.Services;
using Weave.Services.OneNote;
using Weave.Services.Pocket;
using Weave.Settings;
using Weave.UI.Frame;
using Weave.ViewModels;
using Weave.ViewModels.Mobilizer;
using Weave.WP.ViewModels;

namespace weave
{
    public partial class ReadabilityPage : PhoneApplicationPage, IDisposable
    {
        #region Private member variables

        UserInfo user;
        ReadabilityPageViewModel viewModel;
        bool isHtmlDisplayed = false;
        bool isArticleNonDisplayable = false;
        bool isPageClosed = false;
        CompositeDisposable disposables = new CompositeDisposable();
        SerialDisposable setArticleHandle = new SerialDisposable();
        Brush opacityMask;
        SelesGames.PopupService<string> sharePopupService;
        SelesGames.PopupService<Unit> fontSizePopupService;
        FontSizePopup fontSizePopup;
        SocialShareContextMenuControl socialSharePopup;
        PermanentState permState;
        SwipeGestureHelper swipeHelper;
        OverlayFrame frame;

        #endregion




        #region Constructor

        public ReadabilityPage()
        {
            InitializeComponent();

            browser.SoftCollapse();
            browser.IsScriptEnabled = true;
            opacityMask = browser.OpacityMask;
            HideLoadingIndicators();
            fill.SetBinding(Rectangle.FillProperty, new Binding("CurrentTheme.BackgroundBrush") { Source = AppSettings.Instance.Themes });
            this.Loaded += OnLoaded;
            InitializeFlickHandling();

            fontSizePopup = ServiceResolver.Get<FontSizePopup>();
            socialSharePopup = ServiceResolver.Get<SocialShareContextMenuControl>("accent");

            permState = ServiceResolver.Get<PermanentState>();
            var isAppBarMinimized = permState.IsHideAppBarOnArticleViewerPageEnabled;
            ApplicationBar.Mode = isAppBarMinimized ? ApplicationBarMode.Minimized : ApplicationBarMode.Default;
            bottomBarFill.Height = isAppBarMinimized ? 30d : 72d;

            user = ServiceResolver.Get<UserInfo>();
            frame = GlobalNavigationService.CurrentFrame;
        }

        #endregion




        #region Event Handlers

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.OrientationChanged += OnOrientationChanged;
            this.Unloaded += OnUnloaded;
            ChangeBrowserMarginsByPageLayout();
            AppSettings.Instance.Themes.PropertyChanged += OnThemeChanged;
        }

        void BindIsFavoriteToAppBar()
        {
            var favoriteButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            var bindingAdapter = new ApplicationBarToggleIconButtonAdapter(favoriteButton)
            {
                CheckedText = "unfavorite",
                UncheckedText = "favorite",
                CheckedIconUri = new Uri("/Assets/Icons/appbar.heart.png", UriKind.Relative),
                UncheckedIconUri = new Uri("/Assets/Icons/appbar.heart.outline.png", UriKind.Relative),
                IsChecked = viewModel.NewsItem.IsFavorite,
            };
            bindingAdapter.SetBinding(ApplicationBarToggleIconButtonAdapter.IsCheckedProperty,
                 new Binding("IsFavorite") { Source = viewModel.NewsItem, Mode = BindingMode.TwoWay });
            bindingAdapter.DisposeWith(disposables);
            bindingAdapter.IsCheckedChanged += bindingAdapter_IsCheckedChanged;
            Disposable.Create(() => bindingAdapter.IsCheckedChanged -= bindingAdapter_IsCheckedChanged).DisposeWith(disposables);
        }

        void bindingAdapter_IsCheckedChanged(object sender, EventArgs e)
        {
            var adapter = (ApplicationBarToggleIconButtonAdapter)sender;
            if (adapter.IsChecked)
            {
                user.AddFavorite(viewModel.NewsItem).Fire(OnAddFavoriteException);
            }
            else
            {
                user.RemoveFavorite(viewModel.NewsItem).Fire();
            }
        }

        void OnAddFavoriteException(Exception e)
        {
            MessageBox.Show("Add favorite failed - plesae try again");
        }

        void BindIsOrientationLockedToAppBar()
        {
            var orientationLockButton = ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
            var orientationLockService = ServiceResolver.Get<OrientationLockService>();

            var bindingAdapter = new ApplicationBarToggleMenuItemAdapter(orientationLockButton) 
            {
                CheckedText = "turn off \"soft\" rotation lock",
                UncheckedText = "turn on \"soft\" rotation lock",
                IsChecked = orientationLockService.IsLocked,
            };
            bindingAdapter.SetBinding(ApplicationBarToggleMenuItemAdapter.IsCheckedProperty,
                 new Binding("IsLocked") { Source = orientationLockService, Mode = BindingMode.TwoWay });
            bindingAdapter.DisposeWith(disposables);
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

        // Remove event handlers on page unloaded
        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.OrientationChanged -= OnOrientationChanged;
            this.Unloaded -= OnUnloaded;
            AppSettings.Instance.Themes.PropertyChanged -= OnThemeChanged;
        }

        void OnThemeChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CurrentTheme")
                return;

            var theme = AppSettings.Instance.Themes.CurrentTheme;
            ColorToTheme(theme);
        }

        void ColorToTheme(ReadingTheme theme)
        {
            try
            {
                browser.InvokeScript("colorFontAndBackground", theme.Text, theme.Background);
            }
            catch (Exception) { }
        }

        #endregion




        #region Initialize Touch Swiping/Flicking handling

        void InitializeFlickHandling()
        {
            swipeHelper = new SwipeGestureHelper(LayoutRoot);
            swipeHelper.Swipe += OnSwipe;
            swipeHelper.DisposeWith(disposables);
        }

        void OnSwipe(object sender, SwipeGestureHelper.SwipeEventArgs e)
        {
            if (e.Direction == SwipeGestureHelper.SwipeDirection.Left)
                OnLeftSwipe();
            else if (e.Direction == SwipeGestureHelper.SwipeDirection.Right)
                OnRightSwipe();
        }

        void OnLeftSwipe()
        {
            IsHitTestVisible = false;
            SetValue(RadTransitionControl.TransitionProperty, 
                new RadSlideTransition 
                { 
                    PlayMode = TransitionPlayMode.Simultaneously, 
                    Orientation = System.Windows.Controls.Orientation.Horizontal 
                });
            NavigationService.TryGoBack();
        }

        void OnRightSwipe()
        {
            ShowSharingPopup("horizontal");
        }

        #endregion


 

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                if (e.NavigationMode == NavigationMode.New)
                    LoadingInSB.Begin();

                // Check to see if the article was not displayable due to an Exception.  If so, go back to previous page.
                if (isArticleNonDisplayable)
                {
                    NavigationService.TryGoBack();
                    return;
                }

                if (viewModel == null)
                {
                    var isViewModelInit = InitializeViewModel();
                    if (!isViewModelInit)
                    {
                        NavigationService.TryGoBack();
                        return;
                    }
                    else
                        CompleteInitialization();
                }

                if (!isHtmlDisplayed || UserSwitchedViewingType)
                {
                    await DisplayArticleContent();
                    if (isPageClosed)
                        return;

                    SetValue(RadTransitionControl.TransitionProperty, new RadContinuumAndSlideTransition { PlayMode = TransitionPlayMode.Simultaneously });
                    BeginMarkReadTimer();
                }
            }
            catch(Exception ex)
            {
                DebugEx.WriteLine(ex);
                NavigationService.TryGoBack();
            }
        }

        void BeginMarkReadTimer()
        {
            Observable.Timer(TimeSpan.FromSeconds(3)).ObserveOnDispatcher().Subscribe(_ =>
            {        
                if (viewModel != null && viewModel.NewsItem != null && user != null)
                {
                    var newsItem = viewModel.NewsItem;
                    newsItem.HasBeenViewed = true;
                    user.MarkArticleRead(newsItem).Fire();
                }
            })
            .DisposeWith(disposables);
        }

        void CompleteInitialization()
        {
            BindIsFavoriteToAppBar();
            BindIsOrientationLockedToAppBar();
        }

        bool UserSwitchedViewingType
        {
            get
            {
                return viewModel.NewsItem.Feed != null && viewModel.LastViewingType != viewModel.NewsItem.Feed.ArticleViewingType;
            }
        }

        bool InitializeViewModel()
        {
            var ts = ServiceResolver.Get<TombstoneState>();
            var newsItem = ts.CurrentlyViewedNewsItem;
            if (newsItem == null || newsItem.Feed == null)
                return false;

            viewModel = new ReadabilityPageViewModel { NewsItem = newsItem };
            return true;
        }

        async Task DisplayArticleContent()
        {
            bool isMobilizerFaulted = false, isWebViewFaulted = false;

            browser.SoftCollapse();
            browser.OpacityMask = opacityMask;
            ShowLoadingIndicators();
            setArticleHandle.Disposable = null;

            if (viewModel == null || viewModel.NewsItem == null || viewModel.NewsItem.Feed == null)
                return;

            var articleViewType = viewModel.NewsItem.Feed.ArticleViewingType;

            if (articleViewType == ArticleViewingType.Mobilizer || articleViewType == ArticleViewingType.MobilizerOnly)
            {
                isMobilizerFaulted = !(await TryDisplayMobilized());
            }

            if (isPageClosed)
                return;
            
            if (articleViewType == ArticleViewingType.InternetExplorer || articleViewType == ArticleViewingType.InternetExplorerOnly || isMobilizerFaulted)
            {
                if (isMobilizerFaulted)
                {
                    frame.OverlayText = "Switching to webview...";
                    frame.IsLoading = true;
                    isWebViewFaulted = !(await TryDisplayWebView());
                    frame.IsLoading = false;
                }
                else
                {
                    isWebViewFaulted = !(await TryDisplayWebView());
                }
            }

            if (isPageClosed)
                return;

            if (isWebViewFaulted)
            {
                isArticleNonDisplayable = true;
                DisplayInInternetExplorer();

                try
                {
                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();
                }
                catch { }
            }

            isHtmlDisplayed = true;
            browser.OpacityMask = null;
            browser.IsHitTestVisible = true;

            await LoadingOutSB.BeginWithNotification().ToTask();
            setArticleHandle.Disposable = browser.GetNavigating().Subscribe(OnBrowserNavigating);
            HideLoadingIndicators();
        }

        async Task<bool> TryDisplayMobilized()
        {
            try
            {
                await viewModel.LoadMobilizedArticle();

                if (isPageClosed)
                    return false;

                await browser.NavigateToStringAsync(viewModel.FullHtml);
                return true;
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
                return false;
            }
        }

        async Task<bool> TryDisplayWebView()
        {
            try
            {
                viewModel.LastViewingType = ArticleViewingType.InternetExplorer;
                await browser.NavigateAsync(new Uri(viewModel.NewsItem.Link, UriKind.Absolute), null, "User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0");//User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0; XBLWP7; ZuneWP7)                                                                                                                                                                               //User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0; NOKIA; Lumia 900)
                return true;
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
                return false;
            }
        }

        void DisplayInInternetExplorer()
        {
            try
            {
                viewModel.NewsItem.HasBeenViewed = true;
                user.MarkArticleRead(viewModel.NewsItem).Fire();
                SelesGames.Phone.TaskService.ToInternetExplorerTask(viewModel.NewsItem.Link);
            }
            catch { }
        }

        void OnBrowserNavigating(NavigatingEventArgs e)
        {
            var uri = e.Uri;
            if (uri == null)
                return;

            // special youtube rule
            var originalString = uri.OriginalString;
            if (originalString != null && originalString.StartsWith("http://m.youtube.com/#/watch"))
                return;

            e.Cancel = true;
            SelesGames.Phone.TaskService.ToInternetExplorerTask(uri);
        }




        #region Show/Hide loading indicators

        void ShowLoadingIndicators()
        {
            new FrameworkElement[] { fill, BusyIndicator }.ToList().ForEach(o => o.Visibility = Visibility.Visible);
            BusyIndicator.Opacity = 1d;
            BusyIndicator.IsPlaying = true;
        }

        void HideLoadingIndicators()
        {
            new FrameworkElement[] { fill, BusyIndicator }.ToList().ForEach(o => o.Visibility = Visibility.Collapsed);
            BusyIndicator.IsPlaying = false;
        }

        #endregion




        #region Button and Menu Item handling

        void favoriteButton_Click(object sender, EventArgs e)
        {
            viewModel.NewsItem.IsFavorite = true;
        }




        #region Font / Font Size change handlers

        void fontButton_Click(object sender, EventArgs e)
        {
            if (PopupService.IsOpen)
                return;

            fontSizePopupService = new SelesGames.PopupService<System.Reactive.Unit>(fontSizePopup);
            fontSizePopupService.BeginShow();
            Observable.FromEventPattern<EventArgs<FontSizeProperties>>(fontSizePopup, "FontSizeChanged")
                .Subscribe(o => OnFontSizeChanged(fontSizePopup, o.EventArgs)).DisposeWith(disposables);
            Observable.FromEventPattern<EventArgs<FontProperties>>(fontSizePopup, "FontChanged")
                .Subscribe(o => OnFontChanged(fontSizePopup, o.EventArgs)).DisposeWith(disposables);
        }

        void OnFontSizeChanged(object sender, EventArgs<FontSizeProperties> e)
        {
            SetFontSize(e.Item.HtmlTextSize());
        }

        void SetFontSize(string fontSize)
        {
            try
            {
                browser.InvokeScript("setTextSize", fontSize);
            }
            catch (Exception) { }
        }

        void OnFontChanged(object sender, EventArgs<FontProperties> e)
        {
            SetFont(e.Item.FontName);
        }

        void SetFont(string fontName)
        {
            try
            {
                browser.InvokeScript("setFont", fontName);
            }
            catch (Exception) { }
        }

        #endregion




        #region Share Menu and Button Handling

        void shareButton_Click(object sender, EventArgs e)
        {
            ShowSharingPopup("vertical");
        }

        void ShowSharingPopup(string mode)
        {
            if (PopupService.IsOpen)
                return;

            if (mode == "horizontal")
                socialSharePopup.SetHorizontalMode();
            else if (mode == "vertical")
                socialSharePopup.SetVerticalMode();

            sharePopupService = new SelesGames.PopupService<string>(socialSharePopup);
            sharePopupService.BeginShow();
            Observable.FromEventPattern<EventArgs<PopupResult<string>>>(socialSharePopup, "ResultCompleted").Take(1)
                .Subscribe(o => OnSocialSharePopupResultCompleted(socialSharePopup, o.EventArgs)).DisposeWith(disposables);
        }

        void OnSocialSharePopupResultCompleted(object sender, EventArgs<PopupResult<string>> e)
        {
            var result = e.Item.Result;
            OnShareMenuItemClick(result);
        }

        void OnShareMenuItemClick(string menuItem)
        {
            if (viewModel == null || viewModel.NewsItem == null || menuItem == null)
                return;

            var newsItem = viewModel.NewsItem;

            if (menuItem == "email")
                newsItem.ShareToEmail();
            else if (menuItem == "socialShare")
                newsItem.ShareToSocial();
            else if (menuItem == "sms")
                newsItem.ShareToSms();
            else if (menuItem == "instapaper")
                newsItem.SendToInstapaper();
            else if (menuItem == "ie")
                newsItem.SendToInternetExplorer();
            else if (menuItem == "pocket")
                SaveToPocket().Fire();
            else if (menuItem == "onenote")
                SaveToOneNote().Fire();
        }

        #endregion




        void EditSourceAppMenuItemClick(object sender, System.EventArgs e)
        {
            NavigationService.ToEditSourcePage(viewModel.NewsItem.Feed);
        }

        void SpeakArticleAppMenuItemClick(object sender, System.EventArgs e)
        {
            if (viewModel == null || viewModel.NewsItem == null || viewModel.NewsItem.Feed == null)
                return;

            var articleViewType = viewModel.NewsItem.Feed.ArticleViewingType;

            if (!(articleViewType == ArticleViewingType.Mobilizer || articleViewType == ArticleViewingType.MobilizerOnly))
            {
                MessageBox.Show("The 'Speak Article' feature only works with mobilized articles");
                return;
            }

            if (viewModel.CurrentMobilizedArticle == null)
            {
                MessageBox.Show("Please wait until the article has finished downloading");
                return;
            }

            var articleText = viewModel.CurrentMobilizedArticle.CreateSpokenRepresentation();

            var text2speech = new Windows.Phone.Speech.Synthesis.SpeechSynthesizer();
            var voice = new SpeakArticleVoices().GetByDisplayName(permState.SpeakTextVoice);
            if (voice != null && voice.Voice != null)
                text2speech.SetVoice(voice.Voice);

            var speechControls = new weave.Pages.WebBrowser.ArticleSpeechControls();
            var speechControlsPopupService = new SelesGames.PopupService<object>(speechControls);
            speechControlsPopupService.BeginShow();
            browser.Opacity = 0.6d;

            EventHandler stopButtonHandler = null;
            EventHandler<CancelEventArgs> backKeyPressHandler = null;

            stopButtonHandler = (s, e2) =>
            {
                speechControlsPopupService.BeginHide();
                browser.Opacity = 1d;
                text2speech.CancelAll();
                speechControls.StopButtonPressed -= stopButtonHandler;
                this.BackKeyPress -= backKeyPressHandler;
            };
            backKeyPressHandler = (s, e2) =>
            {
                e2.Cancel = true;
                speechControlsPopupService.BeginHide();
                browser.Opacity = 1d;
                text2speech.CancelAll();
                speechControls.StopButtonPressed -= stopButtonHandler;
                this.BackKeyPress -= backKeyPressHandler;
            };
            speechControls.StopButtonPressed += stopButtonHandler;
            this.BackKeyPress += backKeyPressHandler;

            text2speech.SpeakTextAsync(articleText)
                .AsTask()
                .ContinueWith(_ =>
                {
                    speechControlsPopupService.BeginHide();
                    browser.Opacity = 1d;
                    speechControls.StopButtonPressed -= stopButtonHandler;
                    this.BackKeyPress -= backKeyPressHandler;
                }, 
                CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());

            //foreach (var voice in Windows.Phone.Speech.Synthesis.InstalledVoices.All)
            //{
            //    Debug.WriteLine(voice.DisplayName + ", " +
            //        voice.Language + ", " +
            //        voice.Gender + ", " +
            //        voice.Description);
            //    using (text2speech = new Windows.Phone.Speech.Synthesis.SpeechSynthesizer())
            //    {
            //        text2speech.SetVoice(voice);
            //        await text2speech.SpeakTextAsync("Hello world! I'm " + voice.DisplayName + ".");
            //    }
            //}

            /*
             * courtesy andras
             var ssml = new StringBuilder(string.Format(@"<?xml version=""1.0"" encoding=""ISO-8859-1""?>
<speak version=""1.0"" xmlns=""http://www.w3.org/2001/10/synthesis"" xml:lang=""{0}"">", voice.Voice.Language));

            ssml.Append(" <![CDATA[" + articleText + "]]> ");
            ssml.AppendLine("<mark name=\"end\" /></speak>");

            var formattedText = ssml.ToString();*/
        }

        void KeepUnreadMenuItemClick(object sender, System.EventArgs e)
        {
            viewModel.NewsItem.HasBeenViewed = false;
            user.MarkArticleUnread(viewModel.NewsItem).Fire();
        }

        async Task SaveToOneNote()
        {
            if (viewModel == null || viewModel.NewsItem == null)
                return;

            if (!isHtmlDisplayed)
            {
                MessageBox.Show("Please wait until the article has finished downloading");
                return;
            }

            var articleViewType = viewModel.NewsItem.Feed.ArticleViewingType;

            var isMobilized =
                (articleViewType == ArticleViewingType.Mobilizer || articleViewType == ArticleViewingType.MobilizerOnly)
                && viewModel.CurrentMobilizedArticle != null;

            await OneNoteHelper.Current.Save(viewModel.CurrentMobilizedArticle, viewModel.NewsItem, isMobilized);   
        }

        void SendToOneNoteMenuItemClick(object sender, System.EventArgs e)
        {
            SaveToOneNote().Fire();
        }

        async Task SaveToPocket()
        {
            if (viewModel != null && viewModel.NewsItem != null && viewModel.NewsItem.Feed != null)
            {
                var newsItem = viewModel.NewsItem;
                await PocketHelper.Current.Save(newsItem);
            }
        }

        void SaveToPocketMenuItemClick(object sender, EventArgs e)
        {
            SaveToPocket().Fire();
        }



        #endregion




        #region OnBackKeyPress

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (PopupService.IsOpen || isPageClosed)
            {
                e.Cancel = true;
                return;
            }

            isPageClosed = true;

            if (frame.IsLoading)
                frame.IsLoading = false;

            IsHitTestVisible = false;
            base.OnBackKeyPress(e);
        }

        #endregion




        #region IDisposable

        public void Dispose()
        {
            setArticleHandle.Dispose();
            disposables.Dispose();
            this.Loaded -= OnLoaded;
            if (swipeHelper != null)
                swipeHelper.Swipe -= OnSwipe;
        }

        #endregion
    }
}




#region unused saved to isostorage example

        //private void SaveToIsoStore(string fileName, byte[] data)
        //{
        //    string strBaseDir = string.Empty;
        //    string delimStr = "/";
        //    char[] delimiter = delimStr.ToCharArray();
        //    string[] dirsPath = fileName.Split(delimiter);

        //    //Get the IsoStore.
        //    IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();

        //    //Re-create the directory structure.
        //    for (int i = 0; i < dirsPath.Length - 1; i++)
        //    {
        //        strBaseDir = System.IO.Path.Combine(strBaseDir, dirsPath[i]);
        //        isoStore.CreateDirectory(strBaseDir);
        //    }

        //    //Remove the existing file.
        //    if (isoStore.FileExists(fileName))
        //    {
        //        isoStore.DeleteFile(fileName);
        //    }

        //    //Write the file.
        //    using (BinaryWriter bw = new BinaryWriter(isoStore.CreateFile(fileName)))
        //    {
        //        bw.Write(data);
        //        bw.Close();
        //    }
        //}

        //private void SaveToIsoStore(string html)
        //{
        //    var bytes = new UTF8Encoding(false, false).GetBytes(html);
        //    SaveToIsoStore("temp.html", bytes);
        //}
#endregion