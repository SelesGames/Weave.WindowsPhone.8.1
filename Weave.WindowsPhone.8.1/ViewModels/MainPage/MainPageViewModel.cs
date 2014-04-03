using SelesGames;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Weave.LiveTile.ScheduledAgent;
using Weave.LiveTile.ScheduledAgent.ViewModels;
using Weave.Settings;
using Weave.ViewModels;
using Weave.WP.ViewModels.GroupedNews;

namespace Weave.WP.ViewModels.MainPage
{
    public class MainPageViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Private member variables

        static string lastCategory;
        static IScheduler scheduler = ViewModelBackgroundScheduler.Instance;

        int currentPage = 0;
        int numberOfPages = 1;

        List<NewsItem> displayedNews;
        List<NewsItem> previouslyDisplayedNews = new List<NewsItem>();
        weave.MainPage view;
        TombstoneState tombstoneState;
        UserInfo user;
        int lastPageLastTimeItWasSet = -1;
        SerialDisposable pageChangeHandle = new SerialDisposable();

        NewsItemGroup group;
        PagedNewsItems pagedNews;
        IEnumerable<AsyncNewsList> newsLists;

        #endregion




        #region Public Properties

        public string Header { get; private set; }
        public Guid FeedId { get; set; }

        #endregion




        #region Public Derived Properties

        public string CurrentPageDisplay
        {
            get { return string.Format("PAGE {0} OF {1}", currentPage + 1, numberOfPages); }
        }

        #endregion




        #region Constructor

        public MainPageViewModel(weave.MainPage view, string header, NewsItemGroup group)
        {
            this.view = view;
            Header = header;
            this.group = group;
            user = ServiceResolver.Get<UserInfo>();
        }

        #endregion




        #region Getting and Saving TombstoneState

        internal void RecoverSavedTombstoneState()
        {
            tombstoneState = ServiceResolver.Get<TombstoneState>();
            if (tombstoneState == null)
                return;

            if (Header != lastCategory)
                currentPage = 0;

            else
                currentPage = tombstoneState.ArticleListCurrentPage;
        }

        internal void SaveTransientState()
        {
            if (tombstoneState != null)
                tombstoneState.ArticleListCurrentPage = currentPage;
        }

        #endregion




        #region OnNavigatedTo

        public async Task OnNavigatedTo(NavigationMode navMode = NavigationMode.New)
        {
            if (navMode == NavigationMode.Forward || navMode == NavigationMode.New)
            {
                InitializePagedNews();

                var entryType = (Header != lastCategory) ? EntryType.Mark : EntryType.Peek;

                newsLists = pagedNews.GetNewsLists(entryType).Memoize();

                ReevaluateNextAndPreviousButtonsVisibilities();
                await GetNewsForCurrentPage();
                lastCategory = Header;
            }
        }

        #endregion




        #region PagedNews Initialization

        void InitializePagedNews()
        {
            if (group == null)
                throw new Exception("Unable to initialize niGroup in InitializeNewsCollectionVM");

            if (pagedNews != null)
            {
                pagedNews.CountChanged -= pageNews_CountChanged;
            }
            pagedNews = new PagedNewsItems(group, AppSettings.Instance.NumberOfNewsItemsPerMainPage, 3);
            pagedNews.CountChanged += pageNews_CountChanged;
        }

        void pageNews_CountChanged(object sender, EventArgs e)
        {
            numberOfPages = pagedNews.PageCount;
            if (currentPage >= numberOfPages || Header != lastCategory)
                currentPage = 0;

            GlobalDispatcher.Current.BeginInvoke(() => PropertyChanged.Raise(this, "CurrentPageDisplay"));

            ReevaluateNextAndPreviousButtonsVisibilities();
        }

        #endregion




        #region Get News for Current Page

        async Task GetNewsForCurrentPage()
        {
            int pageToRetrieve = currentPage;

            bool showProgressBars = false;
            var newsTask = newsLists.Skip(pageToRetrieve).First().News();

            if (!newsTask.IsCompleted)
                showProgressBars = true;

            if (showProgressBars)
            {
                view.ShowRadialProgressBar();
            }

            List<NewsItem> news = null;
            try
            {
                news = await newsTask;
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
            news = news ?? new List<NewsItem>();

            if (pageToRetrieve != currentPage)
                return;

            if (showProgressBars)
            {
                view.HideRadialProgressBar();
            }

            if (Enumerable.SequenceEqual(news, previouslyDisplayedNews, NewsItemComparer.Instance))
                return;
            
            previouslyDisplayedNews = displayedNews = news;

            if (currentPage >= lastPageLastTimeItWasSet) // we moved forward
                GlobalDispatcher.Current.BeginInvoke(() => view.CompletePageChangeAnimation(displayedNews, 1));

            else
                GlobalDispatcher.Current.BeginInvoke(() => view.CompletePageChangeAnimation(displayedNews, -1));

            lastPageLastTimeItWasSet = currentPage;
        }

        #endregion




        #region Page Change Logic

        void ReevaluateNextAndPreviousButtonsVisibilities()
        {
            HasPrevious = (currentPage - 1) >= 0;
            HasNext = currentPage + 1 < Math.Ceiling((double)pagedNews.TotalNewsCount / (double)pagedNews.PageSize);
            PropertyChanged.Raise(this, "HasPrevious");
            PropertyChanged.Raise(this, "HasNext");
        }

        public bool HasPrevious { get; private set; }
        public bool HasNext { get; private set; }

        public int CurrentPage
        {
            get { return currentPage; }
            set
            {
                if (value >= 0 && value < numberOfPages)
                {
                    currentPage = value;
                    ReevaluateNextAndPreviousButtonsVisibilities();
                    PropertyChanged.Raise(this, "CurrentPageDisplay");
                    pageChangeHandle.Disposable = Observable
                        .Timer(TimeSpan.FromMilliseconds(200), scheduler)
                        .Take(1)
                        .Subscribe(async _ => await GetNewsForCurrentPage());
                }
            }
        }

        #endregion




        #region Mark Page Read

        internal void MarkCurrentPageRead()
        {
            if (displayedNews == null)
                return;

            GlobalDispatcher.Current.BeginInvoke(() =>
            {
                if (displayedNews == null)
                    return;

                foreach (var newsItem in displayedNews)
                    newsItem.HasBeenViewed = true;
            });
            user.MarkArticlesSoftRead(displayedNews)
                .Fire(ex => OnMarkPageReadException(ex, displayedNews));
        }

        void OnMarkPageReadException(Exception e, IEnumerable<NewsItem> displayedNews)
        {
            if (displayedNews != null)
            {
                foreach (var newsItem in displayedNews)
                    newsItem.HasBeenViewed = false;
            }
            MessageBox.Show("Error marking page read.  You may have an issue with your firewall blocking HTTP POSTs");
        }

        #endregion




        #region Refresh

        internal async Task ManualRefresh()
        {
            newsLists = pagedNews.GetNewsLists(EntryType.ExtendRefresh).Memoize();
            ReevaluateNextAndPreviousButtonsVisibilities();
            await GetNewsForCurrentPage();
        }

        #endregion




        #region Live Tile Creation

        public async Task<CycleTileViewModel> CreateLiveTileViewModel()
        {
            var newsWithImages = displayedNews.Where(o => o.HasImage).ToList();
            if (newsWithImages.Count < 2)
                throw new InvalidOperationException("image count");

            var temp = Guid.NewGuid().ToString() + "photo";
            var imageUris = await newsWithImages.Select(o => o.ImageUrl).CreateImageUrisFromNews(temp, TimeSpan.FromSeconds(3));

            return new CycleTileViewModel
            {
                AppName = AppSettings.Instance.AppName.ToUpperInvariant() + " " + Header.ToTitleCase(),
                SmallBackgroundImageUri = new Uri(@"Assets\Tiles\CycleTileSmall.png", UriKind.Relative),
                NewCount = 0,
                ImageIsoStorageUris = imageUris,
            };
        }

        #endregion




        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion




        #region IDisposable and class Destructor

        public void Dispose()
        {
            if (this.pageChangeHandle != null)
                this.pageChangeHandle.Dispose();

            if (pagedNews != null)
            {
                pagedNews.CountChanged -= pageNews_CountChanged;
            }
        }

        ~MainPageViewModel()
        {
            DebugEx.WriteLine("MainPageViewModel {0} was finalized", this.Header);
        }

        #endregion
    }
}