using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weave.ViewModels;

namespace Weave.WP.ViewModels.MainPage
{
    public class AsyncNewsList
    {
        public Func<Task<List<NewsItem>>> News { get; set; }
    }
}
