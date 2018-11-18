namespace GLHDN.ReactiveBuffers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public static class ObservableHelpers
    {
        /// <summary>
        /// Creates an observable from an <see cref="INotifyCollectionChanged"/> of <see cref="INotifyPropertyChanged"/> objects.
        /// </summary>
        /// <param name="collection">The collection to bind to.</param>
        /// <param name="valueSelector">Delegate to transform source object into vertex data.</param>
        public static IObservable<IObservable<TResult>> FromObservableCollection<TItem, TResult>(
            INotifyCollectionChanged collection,
            Func<TItem, TResult> valueSelector) where TItem : INotifyPropertyChanged
        {
            var removalCallbacks = new List<Action>();

            IEnumerable<IObservable<TResult>> addItems(NotifyCollectionChangedEventArgs e)
            {
                for (var i = 0; i < e.NewItems.Count; i++)
                {
                    var item = (TItem)e.NewItems[i];
                    var completion = new Subject<PropertyChangedEventArgs>();
                    removalCallbacks.Insert(e.NewStartingIndex + i, () => completion.OnNext(null));
                    yield return Observable
                        .FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                            handler => (sender, args) => handler(args),
                            handler => item.PropertyChanged += handler,
                            handler => item.PropertyChanged -= handler)
                        .Select(a => valueSelector(item)) // should be valueSelector(sender of a).. okay as long as a reference type
                        .StartWith(valueSelector(item))
                        .TakeUntil(completion);
                }
            };

            void removeItems(NotifyCollectionChangedEventArgs e)
            {
                for (int i = 0; i < e.OldItems.Count; i++)
                {
                    removalCallbacks[e.OldStartingIndex](); // not + i because we've already removed the preceding ones..
                    removalCallbacks.RemoveAt(e.OldStartingIndex); // PERF: potentially slow..
                }
            }

            return Observable
                .FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    handler => (sender, args) => handler(args),
                    handler => collection.CollectionChanged += handler,
                    handler => collection.CollectionChanged -= handler)
                .SelectMany(e =>
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            return addItems(e);

                        case NotifyCollectionChangedAction.Move:
                            throw new NotSupportedException();

                        case NotifyCollectionChangedAction.Remove:
                            removeItems(e);
                            return new IObservable<TResult>[0];

                        case NotifyCollectionChangedAction.Replace:
                            removeItems(e);
                            return addItems(e);

                        case NotifyCollectionChangedAction.Reset:
                            removalCallbacks.ForEach(c => c());
                            removalCallbacks.Clear();
                            return new IObservable<TResult>[0];

                        default:
                            return new IObservable<TResult>[0];
                    }
                });
        }
    }
}
