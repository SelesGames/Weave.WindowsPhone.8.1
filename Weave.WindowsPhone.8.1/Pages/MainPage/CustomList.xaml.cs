using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Weave.Customizability;
using Weave.Settings;
using Weave.ViewModels;

namespace weave
{
    public partial class CustomList : UserControl, IDisposable
    {
        #region Private member variables

        List<BaseNewsItemControl> newsItemsUI;
        SerialDisposable disp = new SerialDisposable();
        Subject<Tuple<object, NewsItem>> newsItemSelected = new Subject<Tuple<object, NewsItem>>();
        Brush transparentBrush, subtleBrush, cardViewIndicatorBrush;
        IDisposable tapHandle;
        IReadOnlyList<NewsItem> displayedNews;

        ImageCache imageCache;

        Guid id;

        static readonly double DISABLED_INDICATOR_OPACITY = 0.33d;

        #endregion




        #region Public Properties

        public int AnimationDirection { get; set; }
        public IObservable<Tuple<object, NewsItem>> NewsItemSelected { get { return newsItemSelected.AsObservable(); } }

        #endregion




        #region Constructor

        public CustomList()
        {
            InitializeComponent();
            transparentBrush = scroller.Background;
            subtleBrush = prevIndicator.Fill;
            cardViewIndicatorBrush = new SolidColorBrush(Colors.Black);

            if (this.IsInDesignMode())
                return;


            this.sp.Children.Remove(this.bottomButtons);

            scroller.Visibility = Visibility.Collapsed;

            imageCache = Resources["imageCache"] as ImageCache;

            prevIndicator.Opacity = nextIndicator.Opacity = DISABLED_INDICATOR_OPACITY;

            id = Guid.NewGuid();
            DebugEx.WriteLine("CustomList {0} created", id.ToString());
        }

        #endregion




        #region Draw the BaseNewsItemControls to the StackPanel

        void ApplyCurrentArticleTheme()
        {
            ApplyThemeToControl();
            InitializeNewsItemControls();
            SetNews(this.displayedNews);
        }

        void ApplyThemeToControl()
        {
            if (ArticleTheme == ArticleListFormatType.Card)
            {
                prevIndicator.Fill = cardViewIndicatorBrush;
                nextIndicator.Fill = cardViewIndicatorBrush;
            }
            else
            {
                prevIndicator.Fill = subtleBrush;
                nextIndicator.Fill = subtleBrush;
            }
        }

        void InitializeNewsItemControls()
        {
            // clean up subscription and dispose previous news items
            if (tapHandle != null)
                tapHandle.Dispose();

            this.sp.Children.Clear();

            DisposeNewsItemControls();

            newsItemsUI =
                Enumerable.Range(0, AppSettings.Instance.NumberOfNewsItemsPerMainPage)
                .Select(notUsed =>
                {
                    var ui = CreateNewsItemControl();
                    ui.ImageCache = imageCache;
                    ui.Visibility = Visibility.Collapsed;
                    sp.Children.Add(ui);
                    return (BaseNewsItemControl)ui;
                })
                .ToList();

            this.sp.Children.Add(this.bottomButtons);
            
            tapHandle = newsItemsUI.Select(o => o.GetTap().Select(_ => Tuple.Create((object)o, o.NewsItem))).Merge()
                .Subscribe(newsItemSelected.OnNext, newsItemSelected.OnError, newsItemSelected.OnCompleted);
        }

        void DisposeNewsItemControls()
        {
            if (newsItemsUI != null)
            {
                foreach (var newsItem in newsItemsUI)
                {
                    if (newsItem is IDisposable)
                        ((IDisposable)newsItem).Dispose();
                }
            }
        }

        BaseNewsItemControl CreateNewsItemControl()
        {
            switch (ArticleTheme)
            {
                case ArticleListFormatType.BigImage:
                    return new BigImageNewsItemControl();

                case ArticleListFormatType.SmallImage:
                    return new MainPageNewsItemUI();

                case ArticleListFormatType.Card:
                    return new CardNewsItemControl();

                case ArticleListFormatType.TextOnly:
                    return new TextOnlyNewsItemControl();

                case ArticleListFormatType.Tiles:
                    return new TileNewsItemControl();

                default:
                    throw new Exception(string.Format(
                        "Selected ArticleListFormat is not currently supported: {0}",
                        ArticleTheme.ToString()));
            }
        }

        #endregion




        #region Set NewsItem for each BaseNewsItemControl on the screen

        //void SetNews(IReadOnlyList<NewsItem> news)
        //{
        //    disp.Disposable = null;
        //    this.displayedNews = news;

        //    if (this.imageCache != null)
        //        imageCache.Flush();

        //    if (this.displayedNews == null)
        //        return;

        //    this.IsHitTestVisible = false;

        //    var animationDelay = TimeSpan.FromSeconds(0.08);
        //    var animationBuffer = 4;

        //    lls.ItemsSource = (IList)news;
        //}

        //void lls_ItemRealized(object sender, Microsoft.Phone.Controls.ItemRealizationEventArgs e)
        //{
        //    DebugEx.WriteLine(e);
        //}

        void SetNews(IReadOnlyList<NewsItem> news)
        {
            disp.Disposable = null;
            this.displayedNews = news;

            if (this.imageCache != null)
                imageCache.Flush();

            scroller.ScrollToVerticalOffset(0);

            if (newsItemsUI == null)
                return;

            this.IsHitTestVisible = false;

            var animationDelay = TimeSpan.FromSeconds(0.08);
            var animationBuffer = 4;

            scroller.Visibility = Visibility.Collapsed;

            newsItemsUI.ForEach(o =>
            {
                o.Visibility = Visibility.Collapsed;
                o.NewsItem = null;
            });

            this.bottomButtons.Visibility = Visibility.Collapsed;
            this.scroller.Visibility = Visibility.Visible;

            if (news == null)
                return;

            var tuples = System.Linq.Enumerable.Zip(newsItemsUI, news, (ui, newsItem) => new { ui, newsItem }).ToList();

            CompositeDisposable disposables = new CompositeDisposable();
            disp.Disposable = disposables;




            #region approach 2, with a double timer and ObserveOnDispatcher - WORKS GREAT AS OF 9/16/11


            var firstX = tuples.Take(animationBuffer).ToList();
            var remainderX = tuples.Skip(animationBuffer).ToList();


            firstX.IntroducePeriod(TimeSpan.Zero, animationDelay, DispatcherScheduler.Current).SafelySubscribe(
                o =>
                {
                    o.ui.NewsItem = o.newsItem;
                    o.ui.Visibility = Visibility.Visible;
                    PlaySlideAnimation(o.ui);
                },
                ex =>
                {
                    this.IsHitTestVisible = true;
                },
                () =>
                {
                    try
                    {
                        remainderX.IntroducePeriod(TimeSpan.Zero, TimeSpan.FromMilliseconds(12d), DispatcherScheduler.Current).SafelySubscribe(
                            o =>
                            {
                                o.ui.NewsItem = o.newsItem;
                                o.ui.Visibility = Visibility.Visible;
                            },
                            ex =>
                            {
                                this.bottomButtons.Visibility = Visibility.Visible;
                                this.IsHitTestVisible = true;
                            },
                            () =>
                            {
                                this.bottomButtons.Visibility = Visibility.Visible;
                                this.IsHitTestVisible = true;
                            })
                        .DisposeWith(disposables);
                    }
                    catch (Exception) { }
                })
                .DisposeWith(disposables);

            #endregion
        }

        void PlaySlideAnimation(BaseNewsItemControl baseNewsItemControl)
        {
            if (AnimationDirection == 99)
                return;

            if (AnimationDirection >= 0)
                baseNewsItemControl.PageRight();
            else
                baseNewsItemControl.PageLeft();
        }

        #endregion




        #region Dependency Properties




        #region ItemsSource

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource",
            typeof(IList),
            typeof(CustomList),
            new PropertyMetadata(OnItemsSourceChanged));

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        static void OnItemsSourceChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var cl = s as CustomList;
            if (cl == null)
                return;

            if (e.NewValue is IReadOnlyList<NewsItem>)
            {
                var news = (IReadOnlyList<NewsItem>)e.NewValue;
                cl.SetNews(news);
            }
            else
            {
                cl.SetNews(null);
            }
        }

        #endregion




        #region ArticleTheme

        public static readonly DependencyProperty ArticleThemeProperty = DependencyProperty.Register(
            "ArticleTheme",
            typeof(ArticleListFormatType),
            typeof(CustomList),
            new PropertyMetadata(ArticleListFormatType.SpecialNoneSelectedFormatType, OnArticleThemeChanged));

        public ArticleListFormatType ArticleTheme
        {
            get { return (ArticleListFormatType)GetValue(ArticleThemeProperty); }
            set { SetValue(ArticleThemeProperty, value); }
        }

        static void OnArticleThemeChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var cl = s as CustomList;
            if (cl == null)
                return;

            if (e.NewValue is ArticleListFormatType && e.NewValue != e.OldValue)
            {
                cl.ApplyCurrentArticleTheme();
            }
        }

        #endregion




        #region IsPreviousIndicatorEnabled

        public static readonly DependencyProperty IsPreviousIndicatorEnabledProperty = DependencyProperty.Register(
            "IsPreviousIndicatorEnabled",
            typeof(bool),
            typeof(CustomList),
            new PropertyMetadata(OnIsPreviousIndicatorEnabledChanged));

        public bool IsPreviousIndicatorEnabled
        {
            get { return (bool)GetValue(IsPreviousIndicatorEnabledProperty); }
            set { SetValue(IsPreviousIndicatorEnabledProperty, value); }
        }

        static void OnIsPreviousIndicatorEnabledChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var cl = s as CustomList;
            if (cl == null)
                return;

            cl.prevIndicator.Opacity = (bool)e.NewValue ? 1d : DISABLED_INDICATOR_OPACITY; 
        }

        #endregion




        #region IsNextIndicatorEnabled

        public static readonly DependencyProperty IsNextIndicatorEnabledProperty = DependencyProperty.Register(
            "IsNextIndicatorEnabled",
            typeof(bool),
            typeof(CustomList),
            new PropertyMetadata(OnIsNextIndicatorEnabledChanged));

        public bool IsNextIndicatorEnabled
        {
            get { return (bool)GetValue(IsNextIndicatorEnabledProperty); }
            set { SetValue(IsNextIndicatorEnabledProperty, value); }
        }

        static void OnIsNextIndicatorEnabledChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var cl = s as CustomList;
            if (cl == null)
                return;

            cl.nextIndicator.Opacity = (bool)e.NewValue ? 1d : DISABLED_INDICATOR_OPACITY;
        }

        #endregion




        #endregion




        #region IDisposable implementation and Finalizer

        public void Dispose()
        {
            DisposeNewsItemControls();
            disp.Dispose();

            if (tapHandle != null)
                tapHandle.Dispose();

            var imageCacheHandle = this.imageCache;
            this.imageCache = null;
            System.Reactive.Concurrency.Scheduler.Default.SafelySchedule(() =>
            {
                if (imageCacheHandle != null)
                {
                    imageCacheHandle.Flush();
                    imageCacheHandle = null;
                }
            });

            DebugEx.WriteLine("CustomList {0} disposed", id.ToString());
        }

        ~CustomList()
        {
            DebugEx.WriteLine("CustomList {0} destroyed", id.ToString());
        }

        #endregion
    }
}