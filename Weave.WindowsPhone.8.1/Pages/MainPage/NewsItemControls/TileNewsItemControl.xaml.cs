using SelesGames.Phone.ValueConverters;
using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Weave.Settings;
using Weave.ViewModels;

namespace weave
{
    public partial class TileNewsItemControl : BaseNewsItemControl, IDisposable
    {
        SerialDisposable disp = new SerialDisposable();

        public TileNewsItemControl()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            title.Text = null;
            feedName.Text = null;
            headlineTxt.Text = null;
            ClearExistingImage();
        }

        protected override void SetNewsItem(NewsItem newsItem)
        {
            disp.Disposable = null;
            var disposables = new CompositeDisposable();
            disp.Disposable = disposables;

            OnLoadSB.Stop();
            OnLoadBackwardsSB.Stop();
            ImageFadeInSB.Stop();

            ClearExistingImage();


            if (newsItem.HasImage)
            {
                headlineTxt.Text = newsItem.Title;

                image.Opacity = 0;
                noImageGrid.Visibility = Visibility.Collapsed;
                withImageGrid.Visibility = Visibility.Visible;

                ImageCache
                    .GetImageAsync(newsItem.ImageUrl)
                    .SafelySubscribe(SetImage, ex => SetImage(FailImage))
                    .DisposeWith(disposables);
            }
            else
            {
                title.Text = newsItem.Title;
                feedName.Text = newsItem.FormattedForMainPageSourceAndDate;

                withImageGrid.Visibility = Visibility.Collapsed;
                noImageGrid.Visibility = Visibility.Visible;
            }

            Binding b = new Binding("DisplayState")
            {
                Converter = new DelegateValueConverter(value =>
                    {
                        var displayState = (NewsItem.ColoringDisplayState)value;
                        return (displayState == NewsItem.ColoringDisplayState.Viewed) ? 0.6d : 1d;
                    },
                    null),
                Source = newsItem
            };

            this.stack.SetBinding(UIElement.OpacityProperty, b);

            ColorByline(newsItem);

            Observable.FromEventPattern<PropertyChangedEventArgs>(newsItem, "PropertyChanged")
                .Where(o => o.EventArgs.PropertyName == "DisplayState")
                .SafelySubscribe(o => ColorByline(newsItem))
                .DisposeWith(disposables);
        }

        void ColorByline(NewsItem newsItem)
        {
            if (newsItem == null || feedName == null)
                return;

            Brush brush;

            if (newsItem.IsFavorite)
                brush = AppSettings.Instance.Themes.CurrentTheme.ComplementaryBrush;
            else if (newsItem.IsDisplayedAsNew)
                brush = AppSettings.Instance.Themes.CurrentTheme.AccentBrush;
            else
                brush = AppSettings.Instance.Themes.CurrentTheme.SubtleBrush;

            feedName.Foreground = brush;
        }

        void ClearExistingImage()
        {
            if (image.Source != null && image.Source is BitmapImage)
            {
                var bi = (BitmapImage)image.Source;
                bi.UriSource = null;
                bi = null;
                image.Source = null;
            }
        }

        void SetImage(ImageSource image)
        {
            this.ImageFadeInSB.Stop();
            this.image.Source = image;
            this.ImageFadeInSB.Begin();
        }

        public override void PageRight()
        {
            OnLoadSB.Begin();
        }

        public override void PageLeft()
        {
            OnLoadBackwardsSB.Begin();
        }

        public void Dispose()
        {
            disp.Dispose();
        }
    }
}