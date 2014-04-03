using SelesGames;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Weave.SavedState;
using Weave.WP.ViewModels.GroupedNews;

namespace Weave.WP.ViewModels
{
    public class NewsItemGroupToMostViewedAdapter : IDisposable
    {
        FeedsToNewsItemGroupAdapter feedsToNewsItemGroupAdapter;

        public ObservableCollection<NewsItemGroup> MostViewed { get; private set; }

        public NewsItemGroupToMostViewedAdapter(FeedsToNewsItemGroupAdapter feedsToNewsItemGroupAdapter)
        {
            this.feedsToNewsItemGroupAdapter = feedsToNewsItemGroupAdapter;

            if (feedsToNewsItemGroupAdapter == null)
                throw new ArgumentNullException("feedsToNewsItemGroupAdapter");

            //feedsToNewsItemGroupAdapter.PropertyChanged += user_PropertyChanged;
            feedsToNewsItemGroupAdapter.Feeds.CollectionChanged += Feeds_CollectionChanged;

            MostViewed = new ObservableCollection<NewsItemGroup>();

            LoadMostViewed();
        }

        void LoadMostViewed()
        {
            MostViewed.Clear();

            var sources = feedsToNewsItemGroupAdapter.Feeds;
            
            var history = ServiceResolver.Get<PermanentState>().RunHistory.GetTallies();

            var sw = System.Diagnostics.Stopwatch.StartNew();

            var x = from source in sources
                    join h in history on source.DisplayName equals h.Label
                    let tempX = new { Source = source, LabelTally = h }
                    orderby tempX.LabelTally.Count descending
                    select tempX.Source;

            var mostViewed = x.Union(sources).Take(4).ToList();

            foreach (var o in mostViewed)
            {
                o.ImageSource = o.GetTeaserPicImageUrl();
                MostViewed.Add(o);
            }

            sw.Stop();
            DebugEx.WriteLine("most viewed took {0} ms to figure out", sw.ElapsedMilliseconds);
        }




        #region Event Handlers

        void Feeds_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            LoadMostViewed();
        }

        //void user_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "Feeds")
        //    {
        //        LoadMostViewed();
        //    }
        //}

        #endregion




        #region IDisposable

        public void Dispose()
        {
            //feedsToNewsItemGroupAdapter.PropertyChanged -= user_PropertyChanged;
            feedsToNewsItemGroupAdapter.Feeds.CollectionChanged -= Feeds_CollectionChanged;
        }

        #endregion
    }
}
