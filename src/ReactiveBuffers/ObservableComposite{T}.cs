using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace GLHDN.ReactiveBuffers
{
    /// <summary>
    /// Composite of observable sequences of leaf data that can be flattened to an observable of observables.
    /// </summary>
    /// <typeparam name="TData">The leaf data type.</typeparam>
    public class ObservableComposite<TData>
    {
        private readonly Subject<TData> removed;
        private readonly Subject<ObservableComposite<TData>> children;
        private readonly HashSet<ObservableComposite<TData>> currentChildren;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableComposite{TData}"/> class.
        /// </summary>
        /// <param name="values">The observable sequence of leaf data for this composite.</param>
        public ObservableComposite(IObservable<TData> values)
        {
            removed = new Subject<TData>();

            Values = values.TakeUntil(removed);

            currentChildren = new HashSet<ObservableComposite<TData>>();
            children = new Subject<ObservableComposite<TData>>();
            Children = Observable.Defer(() => children
                .StartWith(currentChildren.ToArray())
                .TakeUntil(removed));
        }

        internal ObservableComposite(IObservable<TData> values, Dictionary<string, object> monitor, ref int id)
            : this(values)
        {
            monitor?.Add($"item {++id} values", values);
            monitor?.Add($"item {id} children subject", children);
        }

        /// <summary>
        /// Gets the observable sequence of leaf data for this composite.
        /// </summary>
        public IObservable<TData> Values { get; }

        /// <summary>
        /// Gets the observable sequence of children of this composite.
        /// </summary>
        public IObservable<ObservableComposite<TData>> Children { get; }

        /// <summary>
        /// Adds a child to this composite.
        /// </summary>
        /// <param name="child">The child to add.</param>
        public void Add(ObservableComposite<TData> child)
        {
            currentChildren.Add(child);
            children.OnNext(child);
        }

        /// <summary>
        /// Removes a child from this composite.
        /// </summary>
        /// <param name="child">The child to remove.</param>
        /// <returns>True if the child was present to be removed, otherwise false.</returns>
        public bool Remove(ObservableComposite<TData> child)
        {
            if (currentChildren.Remove(child))
            {
                child.Remove();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove this composite from its parent.
        /// </summary>
        public void Remove()
        {
            this.removed.OnNext(default);
        }

        /// <summary>
        /// Flattens this composite into an observable of observables of leaf data.
        /// </summary>
        /// <returns>An observable of observables of leaf data, one for each composite that is a descendent of this one.</returns>
        public IObservable<IObservable<TData>> Flatten()
        {
            void subscribe(IObservable<TData> removed, ObservableComposite<TData> node, IObserver<IObservable<TData>> observer, CompositeDisposable disposable)
            {
                var disposed = new Subject<TData>();
                observer.OnNext(node.Values.TakeUntil(removed.Merge(disposed)));
                disposable.Add(Disposable.Create(() => disposed.OnNext(default)));

                var childrenDisposable = node.Children
                    .TakeUntil(removed)
                    .Subscribe(n => subscribe(removed.Merge(node.Values.TakeLast(1)), n, observer, disposable));
                disposable.Add(childrenDisposable);
            }

            return Observable.Create<IObservable<TData>>(o =>
            {
                var disposable = new CompositeDisposable();
                subscribe(Observable.Never<TData>(), this, o, disposable);
                return disposable;
            });
        }
    }
}
