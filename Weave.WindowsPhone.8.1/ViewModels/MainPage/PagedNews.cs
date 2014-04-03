using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.ViewModels;
using Weave.WP.ViewModels.GroupedNews;

namespace Weave.WP.ViewModels.MainPage
{
    public class PagedNewsItems
    {
        NewsItemGroup vm;

        public event EventHandler CountChanged;

        public int PageSize { get; private set; }
        public int NumberOfPagesToTakeAtATime { get; private set; }
        public int PageCount { get; private set; }
        public int TotalNewsCount { get; private set; }
        public int NewNewsCount { get; private set; }

        public PagedNewsItems(NewsItemGroup vm, int pageSize, int numberOfPagesToTakeAtATime)
        {
            this.vm = vm;
            PageSize = pageSize;
            NumberOfPagesToTakeAtATime = numberOfPagesToTakeAtATime;
            PageCount = 1;
        }

        public IEnumerable<AsyncNewsList> GetNewsLists(EntryType initialEntryType)
        {
            int i = 0;

            while (true)
            {
                var entryType = i == 0 ? initialEntryType : EntryType.Peek;

                var takeAmount = PageSize * NumberOfPagesToTakeAtATime;
                var skipAmount = i * PageSize;
                var load = Lazy.Create(() => GetChunk(entryType, skipAmount, takeAmount));

                for (int j = 0; j < NumberOfPagesToTakeAtATime; j++)
                {
                    var skipMult = j;
                    yield return new AsyncNewsList
                    {
                        News = () => SafelyGetNewsList(load, skipMult),
                    };
                }

                i += NumberOfPagesToTakeAtATime;
            }        
        }

        async Task<NewsList> GetChunk(EntryType entryType, int skip, int take)
        {
            var currentNewsList = await vm.GetNewsList(entryType, skip, take);

            PageCount = currentNewsList.GetPageCount(PageSize);
            TotalNewsCount = currentNewsList.TotalArticleCount;
            NewNewsCount = currentNewsList.NewArticleCount;

            if (CountChanged != null)
                CountChanged(this, EventArgs.Empty);

            return currentNewsList;
        }

        async Task<List<NewsItem>> SafelyGetNewsList(Lazy<Task<NewsList>> load, int skipMult)
        {
            try
            {
                var newsList = await load.Value;
                return newsList.News.Skip(skipMult * PageSize).Take(PageSize).ToList();
            }
            catch
            {
                return new List<NewsItem>();
            }
        }
    }
}
