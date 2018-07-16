namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;

    public class BoundBuffer<TElement, TVertex> where TElement : INotifyPropertyChanged
    {
        private readonly int verticesPerObject;
        private readonly Func<TElement, IList> attributeGetter;
        private readonly IList<int> indices;
        private readonly IVertexArrayObject vao;
        private readonly List<Link> linksByCollectionIndex = new List<Link>();
        private readonly List<Link> linksByBufferIndex = new List<Link>();

        private int objectCapacity;
        
        public BoundBuffer(
            INotifyCollectionChanged collection,
            PrimitiveType primitiveType,
            int verticesPerObject,
            int objectCapacity,
            Func<TElement, TVertex[]> attributeGetter,
            IList<int> indices)
        {
            this.verticesPerObject = verticesPerObject;
            this.attributeGetter = attributeGetter;
            this.indices = indices;
            this.objectCapacity = objectCapacity;
            this.vao = new GlVertexArrayObject(
                primitiveType,
                new Func<GlVertexBufferObject>[] { () => new GlVertexBufferObject(BufferTarget.ArrayBuffer, BufferUsage.DynamicDraw, Array.CreateInstance(typeof(TVertex), objectCapacity * verticesPerObject)) },  // TODO: different VAO ctor to avoid needless large heap allocation 
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
                        var link = new Link(this, linksByCollectionIndex.Count, (TElement)e.NewItems[i]);
                        linksByCollectionIndex.Insert(e.NewStartingIndex + i, link);
                        linksByBufferIndex.Add(link);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException();
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        var removedLink = linksByCollectionIndex[e.OldStartingIndex + i]; // find link for old record
                        var oldItemIndex = removedLink.ItemIndex; // note old item index
                        removedLink.Item = default(TElement); // unlink to remove property changed event handler

                        var bufferEndLink = linksByBufferIndex[linksByCollectionIndex.Count - 1]; // find link for end of buffer
                        bufferEndLink.ItemIndex = oldItemIndex; // Move data to vacated spot
                        linksByBufferIndex[oldItemIndex] = bufferEndLink; // update linksByBufferIndex
                        linksByBufferIndex.RemoveAt(linksByCollectionIndex.Count - 1);

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
                    foreach (var link in linksByCollectionIndex)
                    {
                        link.Item = default(TElement);
                    }
                    linksByCollectionIndex.Clear();
                    linksByBufferIndex.Clear();
                    break;
            }
        }

        private class Link
        {
            private BoundBuffer<TElement, TVertex> parent;
            private int bufferIndex;
            private TElement item; // Wouldn't be needed if collection clear gave us the old items..

            public Link(BoundBuffer<TElement, TVertex> parent, int bufferIndex, TElement item)
            {
                this.parent = parent;
                this.ItemIndex = bufferIndex;
                this.Item = item;
            }

            /// <summary>
            /// Gets or sets the index of the item within the underlying VAO.
            /// </summary>
            public int ItemIndex
            {
                get => bufferIndex;
                set
                {
                    bufferIndex = value;
                    if (item != null)
                    {
                        // could just copy buffer, but lets just reinvoke attr getters for now
                        this.SetItemData(item);
                    }
                }
            }

            public TElement Item
            {
                get => item;
                set
                {
                    if (item != null)
                    {
                        item.PropertyChanged -= ItemPropertyChanged;
                    }

                    item = value;

                    if (item != null)
                    {
                        this.SetItemData(item);
                        item.PropertyChanged += ItemPropertyChanged;
                    }
                }
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
                    throw new InvalidOperationException();
                }

                for (int i = 0; i < vertices.Count; i++)
                {
                    parent.vao.AttributeBuffers[0][ItemIndex * parent.verticesPerObject + i] =
                        vertices[i];
                }

                // Update the index
                for (int i = 0; i < parent.indices.Count; i++)
                {
                    parent.vao.IndexBuffer[ItemIndex * parent.indices.Count + i] = 
                        (uint)(ItemIndex * parent.verticesPerObject + parent.indices[i]);
                }
            }
        }
    }
}
