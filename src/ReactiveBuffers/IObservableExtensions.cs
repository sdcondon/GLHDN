namespace GLHDN.ReactiveBuffers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public static class IObservableExtensions
    {
        /// <summary>
        /// Creates an observable from an <see cref="INotifyCollectionChanged"/> of <see cref="INotifyPropertyChanged"/> objects.
        /// </summary>
        /// <param name="collection">The collection to bind to.</param>
        /// <param name="valueSelector">Delegate to transform collection item into data to emit.</param>
        public static IObservable<IObservable<TResult>> ToObservable<TItem, TResult>(
            this INotifyCollectionChanged collection,
            Func<TItem, TResult> valueSelector) where TItem : INotifyPropertyChanged
        {
            var removalCallbacks = new List<Action>();

            IEnumerable<IObservable<TResult>> addItems(NotifyCollectionChangedEventArgs e)
            {
                for (var i = 0; i < e.NewItems.Count; i++)
                {
                    var item = (TItem)e.NewItems[i];
                    var removal = new Subject<object>(); // TODO: avoid using a Subject. FromAsync/TaskCompletionSource seems like it should work, why doesn't it? 
                    removalCallbacks.Insert(e.NewStartingIndex + i, () => removal.OnNext(null)); // One of several aspects of this method that's not thread (re-entry) safe
                    yield return Observable
                        .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                            handler => item.PropertyChanged += handler,
                            handler => item.PropertyChanged -= handler)
                        .Select(a => valueSelector((TItem)a.Sender))
                        .StartWith(valueSelector(item))
                        .TakeUntil(removal);
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
                .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    handler => collection.CollectionChanged += handler,
                    handler => collection.CollectionChanged -= handler)
                .SelectMany(e =>
                {
                    switch (e.EventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            return addItems(e.EventArgs);

                        case NotifyCollectionChangedAction.Move:
                            throw new NotSupportedException();

                        case NotifyCollectionChangedAction.Remove:
                            removeItems(e.EventArgs);
                            return new IObservable<TResult>[0];

                        case NotifyCollectionChangedAction.Replace:
                            removeItems(e.EventArgs);
                            return addItems(e.EventArgs);

                        case NotifyCollectionChangedAction.Reset:
                            removalCallbacks.ForEach(c => c());
                            removalCallbacks.Clear();
                            return new IObservable<TResult>[0];

                        default:
                            return new IObservable<TResult>[0];
                    }
                });
        }

        /// <summary>
        /// Creates an flat observable of (observables of) leaves from a hierarchical structure.
        /// </summary>
        public static IObservable<IObservable<TLeaf>> FlattenComposite<TIn, TLeaf>(
            this IObservable<TIn> root,
            Func<TIn, IObservable<IObservable<TIn>>> getChildren,
            Func<TIn, TLeaf> getLeafData)
        {
            void subscribeNode(IObservable<TIn> obs, Subject<IObservable<TLeaf>> rootSubject)
            {
                rootSubject.OnNext(obs.Select(getLeafData));
                obs.Subscribe(a => getChildren(a).Subscribe(b => subscribeNode(b.TakeUntil(obs.TakeLast(1)), rootSubject))); // todo: another takeuntil needed? write a test..
            }

            var subject = new Subject<IObservable<TLeaf>>();
            subscribeNode(root, subject);
            // todo end subject when root ends
            return subject;
        }
    }
}
