using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace GLHDN.ReactiveBuffers
{
    public class ObservableComposite<TNode, TData>
    {
        private static int id = 0;

        private readonly Subject<TData> removed;
        private readonly Subject<ObservableComposite<TNode, TData>> children;
        private readonly Func<TNode, IObservable<TData>> getValues;
        private readonly Dictionary<string, object> monitor;

        public ObservableComposite(ObservableComposite<TNode, TData> parent, TNode node, Func<TNode, IObservable<TData>> getValues, Dictionary<string, object> monitor)
        {
            IObservable<TData> parentOrSelfRemoved = this.removed = new Subject<TData>();
            if (parent != null)
            {
                parentOrSelfRemoved = parentOrSelfRemoved.Merge(parent.Values.TakeLast(1));
            }
                
            Node = node;

            this.getValues = getValues;
            var nodeVals = getValues(node);
            Values = nodeVals.TakeUntil(parentOrSelfRemoved);

            children = new Subject<ObservableComposite<TNode, TData>>();
            Children = children.TakeUntil(parentOrSelfRemoved);

            this.monitor = monitor;
            monitor.Add($"item {id++} value subject", nodeVals);
            monitor.Add($"item {id} children subject", children);
        }

        public TNode Node { get; }

        public IObservable<TData> Values { get; }

        public IObservable<ObservableComposite<TNode, TData>> Children { get; }

        public ObservableComposite<TNode, TData> Add(TNode node)
        {
            var child = new ObservableComposite<TNode, TData>(this, node, getValues, monitor);
            children.OnNext(child);
            return child;
        }

        public void Remove()
        {
            this.removed.OnNext(default);
        }

        public IObservable<IObservable<TData>> Flatten()
        {
            IDisposable subscribe(ObservableComposite<TNode, TData> node, IObserver<IObservable<TData>> observer, CompositeDisposable disposable)
            {
                var valueDisp = new CancellationDisposable();
                var valueEnd = new Subject<bool>();
                valueDisp.Token.Register(() => valueEnd.OnNext(false));
                observer.OnNext(node.Values.TakeUntil(valueEnd));
                disposable.Add(valueDisp);

                var childDisp = node.Children.Subscribe(n => subscribe(n, observer, disposable));
                disposable.Add(childDisp); // TODO: This adds the comp disposable to itself..

                return disposable;
            }

            return Observable.Create<IObservable<TData>>(o => subscribe(this, o, new CompositeDisposable()));
        }
    }
}
