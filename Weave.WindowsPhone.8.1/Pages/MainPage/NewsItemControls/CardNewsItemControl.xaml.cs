using SelesGames;
using SelesGames.Phone.ValueConverters;
using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Weave.Settings;
using Weave.ViewModels;

namespace weave
{
    public partial class CardNewsItemControl : BaseNewsItemControl, IDisposable
    {
        BindableMainPageFontStyle bindingSource;
        SerialDisposable disp = new SerialDisposable();
        static Brush defaultHeaderBackgroundBrush;

        public CardNewsItemControl()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            title.Text = null;
            feedName.Text = null;
            mediaTypesIcon.OpacityMask = null;
            ClearExistingImage();

            bindingSource = ServiceResolver.Get<BindableMainPageFontStyle>();

            this.title.SetBinding(TextBlock.FontSizeProperty, bindingSource.TitleSizeBinding);
            this.title.SetBinding(TextBlock.FontFamilyProperty, bindingSource.FontFamilyBinding);

            //this.feedName.SetBinding(TextBlock.FontSizeProperty, bindingSource.PublicationLineSizeBinding);
            //this.feedName.SetBinding(TextBlock.FontFamilyProperty, bindingSource.ThicknessBinding);

            this.Margin = new Thickness(0, 12, 0, 12);

            if (defaultHeaderBackgroundBrush == null)
                defaultHeaderBackgroundBrush = Resources["DefaultHeaderBackgroundBrush"] as Brush;
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

            title.Text = newsItem.Title;
            feedName.Text = newsItem.FormattedForMainPageSourceAndDate;//newsItem.FeedSource.FeedName;

            var mediaTypeImageBrush = newsItem.GetMediaTypeImageBrush();
            if (mediaTypeImageBrush != null)
            {
                mediaTypesIcon.OpacityMask = mediaTypeImageBrush;
                mediaTypesIcon.Visibility = Visibility.Visible;
            }
            else
            {
                mediaTypesIcon.Visibility = Visibility.Collapsed;
            }

            if (newsItem.HasImage)
            {
                image.Opacity = 0;
                imageWrapper.Visibility = Visibility.Visible;

                ImageCache
                    .GetImageAsync(newsItem.ImageUrl)
                    .SafelySubscribe(SetImage, ex => SetImage(FailImage))
                    .DisposeWith(disposables);
            }
            else
            {
                imageWrapper.Visibility = Visibility.Collapsed;
            }

            Binding b = new Binding("DisplayState")
            {
                Converter = new DelegateValueConverter(value =>
                {
                    var displayState = (NewsItem.ColoringDisplayState)value;
                    return (displayState == NewsItem.ColoringDisplayState.Viewed) ? 0.5d : 1d;
                },
                    null),
                Source = newsItem
            };

            this.pack.SetBinding(UIElement.OpacityProperty, b);
            //this.imageWrapper.SetBinding(UIElement.OpacityProperty, b);

            ColorByline(newsItem);

            Observable.FromEventPattern<PropertyChangedEventArgs>(newsItem, "PropertyChanged")
                .Where(o => o.EventArgs.PropertyName == "DisplayState")
                .SafelySubscribe(o => ColorByline(newsItem))
                .DisposeWith(disposables);
        }

        void ColorByline(NewsItem newsItem)
        {
            if (newsItem == null || feedName == null || mediaTypesIcon == null)
                return;

            Brush brush;

            if (newsItem.IsFavorite)
                brush = AppSettings.Instance.Themes.CurrentTheme.ComplementaryBrush;
            else if (newsItem.IsDisplayedAsNew)
                brush = AppSettings.Instance.Themes.CurrentTheme.AccentBrush;
            else
                brush = defaultHeaderBackgroundBrush;

            //headerBorder.Background = brush;
            accent.Fill = brush;
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