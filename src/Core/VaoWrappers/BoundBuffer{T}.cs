﻿namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Encapsulates an OpenGL buffer bound to a particular <see cref="INotifyCollectionChanged"/> object.
    /// </summary>
    /// <typeparam name="TElement">The type of objects in the collection object.</typeparam>
    /// <typeparam name="TVertex">The type of vertex data to e stored in the buffer.</typeparam>
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
            : this(collection, primitiveType, objectCapacity, attributeGetter, indices, DefaultMakeVertexArrayObject)
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
            // TODO: clear the links to remove event handlers
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
                        linksByCollectionIndex[e.OldStartingIndex].Delete(); // not + i because we've already removed the preceding ones..
                        linksByCollectionIndex.RemoveAt(e.OldStartingIndex);
                        // Don't need to do anything with indices because of their constant nature..
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        linksByCollectionIndex[e.NewStartingIndex + i].ReplaceItem((TElement)e.NewItems[i]);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // TODO (if/when resizing is supported): clear buffer data / shrink buffer?
                    foreach (var link in linksByCollectionIndex)
                    {
                        link.Delete();
                    }
                    linksByCollectionIndex.Clear();
                    break;
            }
        }

        private class Link
        {
            private readonly BoundBuffer<TElement, TVertex> parent;
            private int bufferIndex;
            private TElement item; // Wouldn't be needed if collection clear gave us the old items..

            public Link(BoundBuffer<TElement, TVertex> parent, TElement item)
            {
                this.parent = parent;

                this.bufferIndex = this.parent.linksByBufferIndex.Count;
                this.parent.linksByBufferIndex.Add(this);

                SetItem(item);
            }

            public void ReplaceItem(TElement item)
            {
                this.item.PropertyChanged -= ItemPropertyChanged;
                SetItem(item);
            }

            public void Delete()
            {
                this.item.PropertyChanged -= ItemPropertyChanged;

                // Grab the last link by buffer index, remove it
                var lastLink = this.parent.linksByBufferIndex[this.parent.linksByBufferIndex.Count - 1];
                parent.linksByBufferIndex.RemoveAt(lastLink.bufferIndex);

                // If the last link isn't this one, replace this one with it
                if (this.parent.linksByBufferIndex.Count > 0)
                {
                    lastLink.bufferIndex = this.bufferIndex;
                    lastLink.SetBufferContent(); // could just copy buffer (eliminating need for item field use here), but lets just reinvoke attr getters for now
                    parent.linksByBufferIndex[this.bufferIndex] = lastLink;
                }
            }

            private void SetItem(TElement item)
            {
                this.item = item;
                this.SetBufferContent();
                item.PropertyChanged += ItemPropertyChanged;
            }

            private void ItemPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
            {
                this.item = (TElement)sender; // Needed if TElement is a value type..
                this.SetBufferContent();
            }

            private void SetBufferContent()
            {
                var vertices = parent.attributeGetter(item);
                if (vertices.Count != parent.verticesPerObject)
                {
                    throw new InvalidOperationException($"Attribute getter must return correct number of vertices ({parent.verticesPerObject}), but actually returned {vertices.Count}.");
                }

                for (int i = 0; i < vertices.Count; i++)
                {
                    parent.vao.AttributeBuffers[0][bufferIndex * parent.verticesPerObject + i] = vertices[i];
                }

                // Update the index
                for (int i = 0; i < parent.indices.Count; i++)
                {
                    parent.vao.IndexBuffer[bufferIndex * parent.indices.Count + i] = 
                        (uint)(bufferIndex * parent.verticesPerObject + parent.indices[i]);
                }
            }
        }
    }
}
