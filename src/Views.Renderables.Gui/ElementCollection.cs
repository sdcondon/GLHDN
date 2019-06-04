namespace GLHDN.Views.Renderables.Gui
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public class ElementCollection : ICollection<ElementBase>, IObservable<IObservable<ElementBase>>
    {
        private readonly IElementParent owner;
        private readonly Subject<BehaviorSubject<ElementBase>> innerSubject = new Subject<BehaviorSubject<ElementBase>>();
        private readonly LinkedList<BehaviorSubject<ElementBase>> currentList = new LinkedList<BehaviorSubject<ElementBase>>();
        private readonly Dictionary<ElementBase, Action> removalCallbacks = new Dictionary<ElementBase, Action>();

        public ElementCollection(IElementParent owner)
        {
            this.owner = owner;
        }

        /// <inheritdoc />
        public int Count => currentList.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public void Add(ElementBase element)
        {
            // todo: throw if parent != null?
            element.Parent = this.owner;

            var removal = new Subject<object>(); // TODO: avoid using a Subject. FromAsync/TaskCompletionSource seems like it should work, why doesn't it? 
            removalCallbacks.Add(element, () => removal.OnNext(null)); // One of several aspects of this method that's not thread (re-entry) safe

            var obs = Observable
                .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    handler => element.PropertyChanged += handler,
                    handler => element.PropertyChanged -= handler)
                .Select(a => (ElementBase)a.Sender)
                .StartWith(element)
                .TakeUntil(removal);

            var subject = new BehaviorSubject<ElementBase>(element);
            obs.Subscribe(subject);

            var node = currentList.AddLast(subject);
            subject.Subscribe(_ => { }, () => currentList.Remove(node));
            innerSubject.OnNext(subject);
        }

        /// <inheritdoc />
        public bool Remove(ElementBase element)
        {
            if (removalCallbacks.TryGetValue(element, out var callback))
            {
                // TODO: element.Parent = null?
                callback.Invoke();
                removalCallbacks.Remove(element);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            // TODO: this.elements.ForEach(e => e.Parent = null);?
            //this.elements.Clear();
        }

        /// <inheritdoc />
        public bool Contains(ElementBase item) =>
            currentList.Any(a => a.Value.Equals(item));

        /// <inheritdoc />
        public void CopyTo(ElementBase[] array, int arrayIndex) =>
            currentList.Select(a => a.Value).ToList().CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public IEnumerator<ElementBase> GetEnumerator() =>
            currentList.Select(a => a.Value).GetEnumerator();

        /// <inheritdoc />)
        IEnumerator IEnumerable.GetEnumerator() =>
            currentList.Select(a => a.Value).GetEnumerator();

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<IObservable<ElementBase>> observer)
        {
            var disposable = innerSubject.Subscribe(observer);
            foreach (var current in currentList)
            {
                observer.OnNext(current);
            }
            return disposable;
        }
    }
}
