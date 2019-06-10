using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace GLHDN.ReactiveBuffers
{
    public class ObservableComposite<TData>
    {
        private readonly Subject<TData> removed;
        private readonly Subject<ObservableComposite<TData>> children;
        private readonly HashSet<ObservableComposite<TData>> currentChildren;

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

        public IObservable<TData> Values { get; }

        public IObservable<ObservableComposite<TData>> Children { get; }

        public void Add(ObservableComposite<TData> child)
        {
            currentChildren.Add(child);
            children.OnNext(child);
        }

        public bool Remove(ObservableComposite<TData> child)
        {
            if (currentChildren.Remove(child))
            {
                child.Remove();
                return true;
            }

            return false;
        }

        public void Remove()
        {
            this.removed.OnNext(default);
        }

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
