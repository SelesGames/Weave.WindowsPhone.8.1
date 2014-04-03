using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Weave.ViewModels;

namespace weave
{
    public abstract class BaseNewsItemControl : UserControl
    {
        static BitmapImage failImage = new BitmapImage(new Uri("/weave;component/Assets/imageDownloadFailed.jpg", UriKind.Relative));

        public static BitmapImage FailImage
        {
            get { return failImage; }
            set { failImage = value; }
        }

        public ImageCache ImageCache { get; set; }

        protected abstract void SetNewsItem(NewsItem newsItem);

        public abstract void PageRight();

        public abstract void PageLeft();




        #region Dependency Properties (NewsItem)

        public static readonly DependencyProperty NewsItemProperty = DependencyProperty.Register(
            "NewsItem",
            typeof(NewsItem),
            typeof(BaseNewsItemControl),
            new PropertyMetadata(OnNewsItemChanged));

        public NewsItem NewsItem
        {
            get { return (NewsItem)GetValue(NewsItemProperty); }
            set { SetValue(NewsItemProperty, value); }
        }

        static void OnNewsItemChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var ui = s as BaseNewsItemControl;
            if (ui == null)
                return;

            if (e.NewValue is NewsItem)
            {
                var newsItem = e.NewValue as NewsItem;
                ui.SetNewsItem(newsItem);
            }
        }

        #endregion
    }
}
