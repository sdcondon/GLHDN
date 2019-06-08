using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace GLHDN.ReactiveBuffers
{
    public class ObservableComposite<TData>
    {
        private static int id = 0;

        private readonly Subject<TData> removed;
        private readonly Subject<ObservableComposite<TData>> children;
        private readonly Dictionary<string, object> monitor;

        public ObservableComposite(IObservable<TData> values, Dictionary<string, object> monitor)
        {
            removed = new Subject<TData>();
                
            Values = values.TakeUntil(removed);

            children = new Subject<ObservableComposite<TData>>();
            Children = children.TakeUntil(removed);

            this.monitor = monitor;
            monitor?.Add($"item {id++} value subject", values);
            monitor?.Add($"item {id} children subject", children);
        }

        public IObservable<TData> Values { get; }

        public IObservable<ObservableComposite<TData>> Children { get; }

        public ObservableComposite<TData> Add(IObservable<TData> values)
        {
            var child = new ObservableComposite<TData>(values, monitor);
            children.OnNext(child);
            return child;
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

                var childrenDisposable = node.Children.TakeUntil(removed).Subscribe(n => subscribe(removed.Merge(node.Values.TakeLast(1)), n, observer, disposable));
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
