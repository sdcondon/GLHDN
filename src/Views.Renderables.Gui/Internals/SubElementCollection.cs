namespace GLHDN.Views.Renderables.Gui
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;

    internal class SubElementCollection : ICollection<Element>, INotifyCollectionChanged
    {
        private IElementParent owner;
        private ObservableCollection<Element> elements;

        /// <inheritdoc /> from INotifyCollectionChanged
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public SubElementCollection(IElementParent owner)
        {
            this.owner = owner;
            this.elements = new ObservableCollection<Element>();
            this.elements.CollectionChanged += (s, e) => CollectionChanged.Invoke(s, e);
            //this.elements.CollectionChanged += CollectionChanged;
        }

        public int Count => elements.Count;

        public bool IsReadOnly => false;

        public void Add(Element element)
        {
            element.Parent = this.owner;
            elements.Add(element);
        }

        public bool Remove(Element element)
        {
            // todo: clear parents?
            return this.elements.Remove(element);
        }

        public void Clear()
        {
            // todo: clear parents?
            this.elements.Clear();
        }

        public bool Contains(Element item) => elements.Contains(item);

        public void CopyTo(Element[] array, int arrayIndex) => elements.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public IEnumerator<Element> GetEnumerator() => this.elements.GetEnumerator();

        /// <inheritdoc />)
        IEnumerator IEnumerable.GetEnumerator() => this.elements.GetEnumerator();
    }
}
