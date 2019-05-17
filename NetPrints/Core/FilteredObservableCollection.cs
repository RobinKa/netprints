using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace NetPrints.Core
{
    public interface IObservableCollectionView<T> : IReadOnlyList<T>, INotifyCollectionChanged
    {
        public bool Contains(T item);

        public int IndexOf(T item);
    }

    public class FilteredObservableCollection<TFiltered, TOriginal> : IObservableCollectionView<TFiltered> where TFiltered : TOriginal
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private readonly ObservableCollection<TOriginal> original;
        private readonly Func<TFiltered, bool> filter;

        private readonly List<TFiltered> subset;

        public int Count => subset.Count;

        TFiltered IReadOnlyList<TFiltered>.this[int index] => subset[index];

        public TFiltered this[int index] => subset[index];

        public FilteredObservableCollection(ObservableCollection<TOriginal> original, Func<TFiltered, bool> filter)
        {
            Debug.Assert(original != null, "Original collection was null.");
            Debug.Assert(filter != null, "Filter was null.");

            this.original = original;
            this.filter = filter;

            original.CollectionChanged += OnOriginalCollectionChanged;

            subset = original.OfType<TFiltered>().Where(item => filter(item)).ToList();
        }

        public static FilteredObservableCollection<TFiltered, TOriginal> TypeFilter(ObservableCollection<TOriginal> original)
        {
            return new FilteredObservableCollection<TFiltered, TOriginal>(original, item => item is TFiltered);
        }

        private void OnOriginalCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            subset.Clear();
            CollectionChanged?.Invoke(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            subset.AddRange(original.OfType<TFiltered>().Where(item => filter(item)));
            CollectionChanged?.Invoke(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, subset, 0));

            // TODO: Handle inserts / replace correctly.

            /*if (e.Action != NotifyCollectionChangedAction.Reset)
            {
                List<TFiltered> oldItems = null, newItems = null;
                int oldIndex = -1;

                // Remove old items
                if (e.OldItems != null)
                {
                    oldIndex = IndexOf(e.OldItems.OfType<TFiltered>().Last());
                    oldItems = e.OldItems.OfType<TFiltered>().Where(item => subset.Remove(item)).ToList();
                }

                // Add new items if filter applies
                if (e.NewItems != null)
                {
                    newItems = e.NewItems.OfType<TFiltered>().Where(item => filter(item)).ToList();

                    foreach (var newItem in newItems)
                    {
                        subset.Add(newItem);
                    }
                }

                // Trigger collection change event if any items were added or removed.
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (newItems != null && newItems.Count > 0)
                        {
                            CollectionChanged?.Invoke(sender, new NotifyCollectionChangedEventArgs(e.Action, newItems, subset.Count - newItems.Count));
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (oldItems != null && oldItems.Count > 0)
                        {
                            CollectionChanged?.Invoke(sender, new NotifyCollectionChangedEventArgs(e.Action, oldItems, oldIndex));
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        if ((oldItems != null && oldItems.Count > 0) || (newItems != null && newItems.Count > 0))
                        {
                            CollectionChanged?.Invoke(sender, new NotifyCollectionChangedEventArgs(e.Action, oldItems, newItems));
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }

            }
            else
            {
                if (subset.Count > 0)
                {
                    subset.Clear();
                    CollectionChanged?.Invoke(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }*/
        }

        public IEnumerator<TFiltered> GetEnumerator() => subset.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => subset.GetEnumerator();

        public bool Contains(TFiltered item) => subset.Contains(item);

        public int IndexOf(TFiltered item) => subset.IndexOf(item);
    }
}
