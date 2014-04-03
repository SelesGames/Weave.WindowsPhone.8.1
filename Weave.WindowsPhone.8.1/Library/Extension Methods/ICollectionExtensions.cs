using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.ViewModels
{
    internal static class ICollectionExtensions
    {
        public static void OrderedUniqueInsert<T, TOrder>(this IList<T> sourceList,
            IList<T> insertItems,
            Func<T, TOrder> orderKeySelector,
            IEqualityComparer<T> equalityComparer = null)

            where TOrder : IComparable
        {
            if (sourceList == null)
                throw new ArgumentNullException("sourceList in OrderedUniqueInsert");

            if (insertItems == null)
                return;

            var newEntryIndex = 0;

            IList<T> proper;

            if (equalityComparer == null)
                proper = insertItems.Except(sourceList).OrderBy(orderKeySelector).ToList();
            else
                proper = insertItems.Except(sourceList, equalityComparer).OrderBy(orderKeySelector).ToList();

            for (int i = 0; i < sourceList.Count; i++)
            {
                if (newEntryIndex >= proper.Count)
                    break;

                var current = proper[newEntryIndex];
                var newsItem = sourceList[i];

                if (orderKeySelector(current).CompareTo(orderKeySelector(newsItem)) < 0)
                {
                    sourceList.Insert(i, current);
                    newEntryIndex++;
                }
            }

            // add any remaining items to the end of the list
            for (; newEntryIndex < proper.Count; newEntryIndex++)
            {
                var current = proper[newEntryIndex];
                sourceList.Add(current);
            }
        }

        public static void OrderedDescendingUniqueInsert<T, TOrder>(this IList<T> sourceList,
            IList<T> insertItems,
            Func<T, TOrder> orderKeySelector,
            IEqualityComparer<T> equalityComparer = null)

            where TOrder : IComparable
        {
            if (sourceList == null)
                throw new ArgumentNullException("sourceList in OrderedDescendingUniqueInsert");

            if (insertItems == null)
                return;

            var newEntryIndex = 0;

            IList<T> proper;

            if (equalityComparer == null)
                proper = insertItems.Except(sourceList).OrderByDescending(orderKeySelector).ToList();
            else
                proper = insertItems.Except(sourceList, equalityComparer).OrderByDescending(orderKeySelector).ToList();

            for (int i = 0; i < sourceList.Count; i++)
            {
                if (newEntryIndex >= proper.Count)
                    break;

                var current = proper[newEntryIndex];
                var newsItem = sourceList[i];

                if (orderKeySelector(current).CompareTo(orderKeySelector(newsItem)) > 0)
                {
                    sourceList.Insert(i, current);
                    newEntryIndex++;
                }
            }

            // add any remaining items to the end of the list
            for (; newEntryIndex < proper.Count; newEntryIndex++)
            {
                var current = proper[newEntryIndex];
                sourceList.Add(current);
            }
        }
    }
}
