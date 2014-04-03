using System;
using System.Collections.Generic;
using Weave.ViewModels;

namespace Weave.WP.ViewModels
{
    public class NewsItemComparer : IEqualityComparer<NewsItem>
    {
        public bool Equals(NewsItem x, NewsItem y)
        {
            return x.Title.Equals(y.Title) && RelaxedDateTimeEquality(x.LocalDateTime, y.LocalDateTime);
        }

        public int GetHashCode(NewsItem obj)
        {
            return obj.Title.GetHashCode();
        }

        bool RelaxedDateTimeEquality(DateTime x, DateTime y) // if it is 10 minutes diff or less, treat as equal
        {
            return Math.Abs((x - y).TotalHours) <= 48d;
        }

        static NewsItemComparer instance = new NewsItemComparer();
        public static NewsItemComparer Instance { get { return instance; } }
    }
}
