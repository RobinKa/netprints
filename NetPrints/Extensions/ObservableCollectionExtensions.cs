using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NetPrints.Extensions
{
    public static class ObservableCollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> obsColl, IEnumerable<T> toAdd)
        {
            foreach (T t in toAdd)
            {
                obsColl.Add(t);
            }
        }

        public static void RemoveRange<T>(this ObservableCollection<T> obsColl, IEnumerable<T> toRemove)
        {
            foreach (T t in toRemove)
            {
                obsColl.Remove(t);
            }
        }
    }
}
