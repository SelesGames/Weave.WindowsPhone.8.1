using System.Windows;
using System.Windows.Controls;
using Weave.ViewModels;

namespace weave
{
    public partial class NewsItemWithImageTile3 : UserControl
    {
        static Style noImageStyle;
        static Style withImageStyle;

        public NewsItemWithImageTile3()
        {
            InitializeComponent();
            if (noImageStyle == null)
                noImageStyle = Resources["NoImageButtonStyle"] as Style;
            if (withImageStyle == null)
                withImageStyle = Resources["WithImageButtonStyle"] as Style;
        }

        internal void PlayAnimation()
        {
            OnLoadSB.Begin();
        }




        #region Dependency Properties

        public static readonly DependencyProperty NewsItemProperty = DependencyProperty.Register(
            "NewsItem",
            typeof(NewsItem),
            typeof(NewsItemWithImageTile3),
            new PropertyMetadata(new PropertyChangedCallback(OnNewsItemChanged)));

        public NewsItem NewsItem
        {
            get { return (NewsItem)GetValue(NewsItemProperty); }
            set { SetValue(NewsItemProperty, value); }
        }

        static void OnNewsItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NewsItemWithImageTile3 tile = (NewsItemWithImageTile3)d;
            var newsItem = (NewsItem)e.NewValue;
            if (newsItem.HasImage)
                tile.button.Style = withImageStyle;
            else
                tile.button.Style = noImageStyle;
            tile.DataContext = newsItem;
        }

        #endregion
    }
}