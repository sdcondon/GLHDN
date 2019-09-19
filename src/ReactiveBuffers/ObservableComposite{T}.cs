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
    /// <typeparam name="T">The leaf data type.</typeparam>
    /// <remarks>
    /// Sits somewhat awkwardly between the imperative and reactive worlds, in that it is updated imperatively, but
    /// exposes a reactive-valued method. Might be slightly cleaner to live entirely in reactive world, especially if
    /// we change user input to be entirely reactive. Meh. For later consideration.
    /// </remarks>
    public class ObservableComposite<T>
    {
        private readonly Subject<T> removed;
        private readonly Subject<ObservableComposite<T>> children;
        private readonly HashSet<ObservableComposite<T>> currentChildren;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableComposite{TData}"/> class.
        /// </summary>
        /// <param name="values">The observable sequence of leaf data for this composite.</param>
        public ObservableComposite(IObservable<T> values)
        {
            removed = new Subject<T>();

            Values = values.TakeUntil(removed);

            currentChildren = new HashSet<ObservableComposite<T>>();
            children = new Subject<ObservableComposite<T>>();
            Children = Observable.Defer(() => children
                .StartWith(currentChildren.ToArray())
                .TakeUntil(removed));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableComposite{TData}"/> class for unit tests.
        /// </summary>
        /// <param name="values">The observable sequence of leaf data for this composite.</param>
        /// <param name="registerField">Delegate to register subject-valued field for subscriber monitoring.</param>
        internal ObservableComposite(IObservable<T> values, Action<string, object> registerField)
            : this(values)
        {
            registerField("values", values);
            registerField("children", children);
        }

        /// <summary>
        /// Gets the observable sequence of leaf data for this composite.
        /// </summary>
        public IObservable<T> Values { get; }

        /// <summary>
        /// Gets the observable sequence of children of this composite.
        /// </summary>
        public IObservable<ObservableComposite<T>> Children { get; }

        /// <summary>
        /// Adds a child to this composite.
        /// </summary>
        /// <param name="child">The child to add.</param>
        public void Add(ObservableComposite<T> child)
        {
            currentChildren.Add(child);
            children.OnNext(child);
        }

        /// <summary>
        /// Removes a child from this composite.
        /// </summary>
        /// <param name="child">The child to remove.</param>
        /// <returns>True if the child was present to be removed, otherwise false.</returns>
        public bool Remove(ObservableComposite<T> child)
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
        public IObservable<IObservable<T>> Flatten()
        {
            void Subscribe(IObservable<T> removed, ObservableComposite<T> node, IObserver<IObservable<T>> observer, CompositeDisposable disposable)
            {
                var disposed = new Subject<T>();
                observer.OnNext(node.Values.TakeUntil(removed.Merge(disposed)));
                disposable.Add(Disposable.Create(() => disposed.OnNext(default)));

                var childrenDisposable = node.Children
                    .TakeUntil(removed)
                    .Subscribe(n => Subscribe(removed.Merge(node.Values.TakeLast(1)), n, observer, disposable));
                disposable.Add(childrenDisposable);
            }

            return Observable.Create<IObservable<T>>(o =>
            {
                var disposable = new CompositeDisposable();
                Subscribe(Observable.Never<T>(), this, o, disposable);
                return disposable;
            });
        }
    }
}
