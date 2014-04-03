using SelesGames.Phone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using Weave.ViewModels;

namespace weave
{
    public partial class CategoryDisplayGrid : UserControl
    {
        List<NewsItemWithImageTile3> displayers;

        public IObservable<NewsItem> NewsItemClicked { get; private set; }

        public CategoryDisplayGrid()
        {
            InitializeComponent();
            
            if (this.IsInDesignMode())
                return;
            
            displayers = new List<NewsItemWithImageTile3>
            {
                // left 4
                item1,
                item2,
                item5,
                item6,

                // right 4
                item3,
                item4,
                item7,
                item8
            };
            Hide();
            NewsItemClicked = displayers.Select(o => o.GetTap().Select(x => o.NewsItem)).Merge();
        }

        void SetNews(IEnumerable<NewsItem> news)
        {
            Hide();

            System.Linq.Enumerable.Zip(news, displayers, (newsItem, display) =>
                new Action(() =>
                {
                    display.NewsItem = newsItem;
                    display.Opacity = 1;
                    display.PlayAnimation();
                }))
                .Take(8)
                .IntroducePeriod(0.1.Seconds())
                .ObserveOnDispatcher()
                .Subscribe(action => action());
        }

        void Hide()
        {
            displayers.ForEach(o => o.Opacity = 0);
        }




        #region News Dependency Property (calls SetNews internally)

        public static readonly DependencyProperty NewsProperty =
            DependencyProperty.Register("News", typeof(IEnumerable<NewsItem>), typeof(CategoryDisplayGrid), new PropertyMetadata(OnNewsChanged));

        public IEnumerable<NewsItem> News
        {
            get { return (IEnumerable<NewsItem>)GetValue(NewsProperty); }
            set { SetValue(NewsProperty, value); }
        }

        static void OnNewsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = (CategoryDisplayGrid)obj;

            if (e.NewValue is IEnumerable<NewsItem>)
            {
                var newNews = (IEnumerable<NewsItem>)e.NewValue;
                var oldNews = e.OldValue as IEnumerable<NewsItem>;

                if (oldNews == null || !Enumerable.SequenceEqual(newNews, oldNews))
                {
                    control.SetNews(newNews);
                }
            }
        }

        #endregion
    }
}