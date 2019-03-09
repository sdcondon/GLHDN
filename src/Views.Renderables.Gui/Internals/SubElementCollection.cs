namespace GLHDN.Views.Renderables.Gui
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;

    internal class SubElementCollection : ICollection<Element>, INotifyCollectionChanged
    {
        private readonly IElementParent owner;
        private readonly ObservableCollection<Element> elements;

        /// <inheritdoc /> from INotifyCollectionChanged
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public SubElementCollection(IElementParent owner)
        {
            this.owner = owner;
            this.elements = new ObservableCollection<Element>();
            this.elements.CollectionChanged += (s, e) => CollectionChanged.Invoke(s, e);
            //this.elements.CollectionChanged += CollectionChanged;
        }

        /// <inheritdoc />
        public int Count => elements.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public void Add(Element element)
        {
            element.Parent = this.owner;
            elements.Add(element);
        }

        /// <inheritdoc />
        public bool Remove(Element element)
        {
            // TODO: element.Parent = null?
            return this.elements.Remove(element);
        }

        /// <inheritdoc />
        public void Clear()
        {
            // TODO: this.elements.ForEach(e => e.Parent = null);?
            this.elements.Clear();
        }

        /// <inheritdoc />
        public bool Contains(Element item) => elements.Contains(item);

        /// <inheritdoc />
        public void CopyTo(Element[] array, int arrayIndex) => elements.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public IEnumerator<Element> GetEnumerator() => this.elements.GetEnumerator();

        /// <inheritdoc />)
        IEnumerator IEnumerable.GetEnumerator() => this.elements.GetEnumerator();
    }
}
