namespace GLHDN.ReactiveBuffers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    /// <summary>
    /// Extension methods for <see cref="INotifyCollectionChanged"/> instances.
    /// </summary>
    public static class INotifyCollectionChangedExtensions
    {
        /// <summary>
        /// Creates an observable from an <see cref="INotifyCollectionChanged"/> of <see cref="INotifyPropertyChanged"/> objects.
        /// </summary>
        /// <typeparam name="T">The type of objects that the collection contains.</typeparam>
        /// <param name="collection">The collection to bind to.</param>
        /// <returns>An observable of observables. The outer observable pushes whenever an element is added to the collection. Each inner observable pushes whenever the element changes, and completes when it is removed.</returns>
        public static IObservable<IObservable<T>> ToObservable<T>(this INotifyCollectionChanged collection)
            where T : INotifyPropertyChanged
        {
            var removalCallbacks = new List<Action>();

            IEnumerable<IObservable<T>> AddItems(NotifyCollectionChangedEventArgs e)
            {
                for (var i = 0; i < e.NewItems.Count; i++)
                {
                    var item = (T)e.NewItems[i];
                    var removal = new Subject<object>(); // TODO: avoid using a Subject. FromAsync/TaskCompletionSource seems like it should work, why doesn't it?
                    removalCallbacks.Insert(e.NewStartingIndex + i, () => removal.OnNext(null)); // One of several aspects of this method that's not thread (re-entry) safe
                    yield return Observable
                        .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                            handler => item.PropertyChanged += handler,
                            handler => item.PropertyChanged -= handler)
                        .Select(a => (T)a.Sender)
                        .StartWith(item)
                        .TakeUntil(removal);
                }
            }

            void RemoveItems(NotifyCollectionChangedEventArgs e)
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
                            return AddItems(e.EventArgs);

                        case NotifyCollectionChangedAction.Move:
                            throw new NotSupportedException();

                        case NotifyCollectionChangedAction.Remove:
                            RemoveItems(e.EventArgs);
                            return new IObservable<T>[0];

                        case NotifyCollectionChangedAction.Replace:
                            RemoveItems(e.EventArgs);
                            return AddItems(e.EventArgs);

                        case NotifyCollectionChangedAction.Reset:
                            removalCallbacks.ForEach(c => c());
                            removalCallbacks.Clear();
                            return new IObservable<T>[0];

                        default:
                            return new IObservable<T>[0];
                    }
                });
        }
    }
}
