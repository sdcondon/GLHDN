namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// Encapsulates an OpenGL buffer bound to a particular <see cref="INotifyCollectionChanged"/> object.
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    /// <typeparam name="TVertex"></typeparam>
    public sealed class BoundBuffer<TElement, TVertex> where TElement : INotifyPropertyChanged
    {
        private readonly int verticesPerObject;
        private readonly Func<TElement, IList<TVertex>> attributeGetter;
        private readonly IList<int> indices;
        private readonly IVertexArrayObject vao;
        private readonly List<Link> linksByCollectionIndex = new List<Link>();
        private readonly List<Link> linksByBufferIndex = new List<Link>();

        private int objectCapacity;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundBuffer"/> class.
        /// </summary>
        /// <param name="collection">The collection to bind to.</param>
        /// <param name="primitiveType">The type of primitive to be drawn.</param>
        /// <param name="objectCapacity">The capacity for the buffer, in objects.</param>
        /// <param name="attributeGetter">Delegate to transform source object into vertex data.</param>
        /// <param name="indices"></param>
        public BoundBuffer(
            INotifyCollectionChanged collection,
            PrimitiveType primitiveType,
            int objectCapacity,
            Func<TElement, IList<TVertex>> attributeGetter,
            IList<int> indices)
            : this(
                collection,
                primitiveType,
                objectCapacity,
                attributeGetter,
                indices,
                DefaultMakeVertexArrayObject)
        {
        }

        internal BoundBuffer(
            INotifyCollectionChanged collection,
            PrimitiveType primitiveType,
            int objectCapacity,
            Func<TElement, IList<TVertex>> attributeGetter,
            IList<int> indices,
            Func<PrimitiveType, IList<Tuple<BufferUsage, Array>>, uint[], IVertexArrayObject> makeVertexArrayObject)
        {
            this.verticesPerObject = indices.Max() + 1;
            this.attributeGetter = attributeGetter;
            this.indices = indices;
            this.objectCapacity = objectCapacity;
            this.vao = makeVertexArrayObject(
                primitiveType,
                new[] { Tuple.Create(BufferUsage.DynamicDraw, Array.CreateInstance(typeof(TVertex), objectCapacity * verticesPerObject)) },  // TODO: different VAO ctor to avoid needless large heap allocation 
                new uint[objectCapacity * indices.Count]); // TODO: different VAO ctor to avoid needless large heap allocation
            collection.CollectionChanged += Collection_CollectionChanged;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.vao.Dispose();
        }

        /// <summary>
        /// Draw with the active program.
        /// </summary>
        public void Draw()
        {
            this.vao.Draw(linksByCollectionIndex.Count * indices.Count);
        }

        private static IVertexArrayObject DefaultMakeVertexArrayObject(PrimitiveType primitiveType, IList<Tuple<BufferUsage, Array>> attributeBufferSpecs, uint[] indices)
        {
            return new GlVertexArrayObject(
                primitiveType,
                attributeBufferSpecs, // TODO: different VAO ctor to avoid needless large heap allocation 
                indices); // TODO: different VAO ctor to avoid needless large heap allocation
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (linksByCollectionIndex.Count + e.NewItems.Count > objectCapacity)
                    {
                        // TODO: expand (i.e. recreate) buffer? ResizeBuffer method in GlVertexArrayObject:
                        // Create new, glCopyBufferSubData, delete old, update buffer ID array. 
                        throw new InvalidOperationException("Insufficient space left in buffer");
                    }

                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var link = new Link(this, (TElement)e.NewItems[i]);
                        linksByCollectionIndex.Insert(e.NewStartingIndex + i, link);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException();
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        var link = linksByCollectionIndex[e.OldStartingIndex]; // not + i because we've already removed the preceding ones..
                        linksByCollectionIndex.RemoveAt(e.OldStartingIndex);
                        link.Remove();
                        // Don't think need to do anything with indices because of their constant nature..
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        linksByCollectionIndex[e.NewStartingIndex + i].Item = (TElement)e.NewItems[i];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // TODO (if/when resizing is supported): clear buffer data / shrink buffer?
                    linksByCollectionIndex.Clear();
                    foreach (var link in linksByCollectionIndex)
                    {
                        link.Remove();
                    }
                    break;
            }
        }

        private class Link
        {
            private BoundBuffer<TElement, TVertex> parent;
            private int bufferIndex;
            private TElement item; // Wouldn't be needed if collection clear gave us the old items..

            public Link(BoundBuffer<TElement, TVertex> parent, TElement item)
            {
                this.parent = parent;

                this.bufferIndex = this.parent.linksByBufferIndex.Count;
                this.parent.linksByBufferIndex.Add(this);

                SetItem(item);
            }

            /// <summary>
            /// Gets the index of the item within the underlying VAO.
            /// </summary>
            public int BufferIndex
            {
                get => bufferIndex;
                private set
                {
                    if (bufferIndex != parent.linksByBufferIndex.Count - 1)
                    {
                        throw new InvalidOperationException("Buffer objects can only be moved from the end");
                    }

                    parent.linksByBufferIndex.RemoveAt(bufferIndex);

                    bufferIndex = value;
                    this.SetItemData(item); // could just copy buffer, but lets just reinvoke attr getters for now
                    this.parent.linksByBufferIndex[value] = this;
                }
            }

            public TElement Item
            {
                get => item;
                set
                {
                    item.PropertyChanged -= ItemPropertyChanged;
                    SetItem(value);
                }
            }

            public void Remove()
            {
                this.item.PropertyChanged -= ItemPropertyChanged;
                this.parent.linksByBufferIndex[this.parent.linksByBufferIndex.Count - 1].BufferIndex = this.BufferIndex; // Move last buffer content to vacated spot
            }

            private void SetItem(TElement item)
            {
                this.item = item;
                this.SetItemData(item);
                this.item.PropertyChanged += ItemPropertyChanged;
            }

            private void ItemPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
            {
                this.SetItemData((TElement)sender);
            }

            /// <summary>
            /// Updates attribute buffers in the underlying VAO from the given item's properties.
            /// </summary>
            /// <param name="item">The item.</param>
            private void SetItemData(TElement item)
            {
                var vertices = parent.attributeGetter(item);
                if (vertices.Count != parent.verticesPerObject)
                {
                    throw new InvalidOperationException($"Attribute getter must return correct number of vertices ({parent.verticesPerObject}), but actually returned {vertices.Count}.");
                }

                for (int i = 0; i < vertices.Count; i++)
                {
                    parent.vao.AttributeBuffers[0][BufferIndex * parent.verticesPerObject + i] = vertices[i];
                }

                // Update the index
                for (int i = 0; i < parent.indices.Count; i++)
                {
                    parent.vao.IndexBuffer[BufferIndex * parent.indices.Count + i] = 
                        (uint)(BufferIndex * parent.verticesPerObject + parent.indices[i]);
                }
            }
        }
    }
}
