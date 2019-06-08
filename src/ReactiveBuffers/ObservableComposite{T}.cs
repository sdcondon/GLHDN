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

        public ObservableComposite(ObservableComposite<TData> parent, IObservable<TData> values, Dictionary<string, object> monitor)
        {
            IObservable<TData> parentOrSelfRemoved = this.removed = new Subject<TData>();
            if (parent != null)
            {
                parentOrSelfRemoved = parentOrSelfRemoved.Merge(parent.Values.TakeLast(1));
            }
                
            Values = values.TakeUntil(parentOrSelfRemoved);

            this.children = new Subject<ObservableComposite<TData>>();
            Children = children.TakeUntil(parentOrSelfRemoved);

            this.monitor = monitor;
            monitor?.Add($"item {id++} value subject", values);
            monitor?.Add($"item {id} children subject", children);
        }

        public IObservable<TData> Values { get; }

        public IObservable<ObservableComposite<TData>> Children { get; }

        public ObservableComposite<TData> Add(IObservable<TData> values)
        {
            var child = new ObservableComposite<TData>(this, values, monitor);
            children.OnNext(child);
            return child;
        }

        public void Remove()
        {
            this.removed.OnNext(default);
        }

        public IObservable<IObservable<TData>> Flatten()
        {
            IDisposable subscribe(ObservableComposite<TData> node, IObserver<IObservable<TData>> observer, CompositeDisposable disposable)
            {
                var disposed = new Subject<bool>();
                observer.OnNext(node.Values.TakeUntil(disposed));
                disposable.Add(Disposable.Create(() => disposed.OnNext(true)));

                disposable.Add(node.Children.Subscribe(n => subscribe(n, observer, disposable)));

                return disposable;
            }

            return Observable.Create<IObservable<TData>>(o => subscribe(this, o, new CompositeDisposable()));
        }
    }
}
