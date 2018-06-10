﻿namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    //TODO: using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// Wraps a <see cref="GlVertexArrayObject"/> that stores vertex data for a collection of objects.
    /// Presents simply as an <see cref="ICollection{T}"/> of the objects.
    /// Updates the backing VAO appropriately as the collection (TODO: and the items within it) is modified.
    /// </summary>
    /// <remarks>
    /// TODO: Maybe play with re-implementing using persistently mapped buffers at some point..
    /// </remarks>
    public sealed class ObjectBuffer<T> : ICollection<T>, IDisposable // TODO: where T : INotifyPropertyChanged
    {
        private readonly Dictionary<T, int> objects = new Dictionary<T, int>();
        private readonly GlVertexArrayObject vao;
        private readonly int verticesPerAtom;
        private readonly IList<Func<T, IList>> attributeGetters;
        private readonly IList<int> indices;

        private int atomCount;
        private int atomCapacity;

        internal ObjectBuffer(
            PrimitiveType primitiveType,
            int verticesPerAtom,
            IList<Type> attributeTypes,
            IList<Func<T, IList>> attributeGetters,
            IList<int> indices,
            int atomCapacity)
        {
            this.vao = new GlVertexArrayObject(
                primitiveType,
                attributeTypes.Select(a => BufferUsage.DynamicDraw).ToArray(),
                attributeTypes.Select(a => Array.CreateInstance(a, atomCapacity * verticesPerAtom)).ToArray(), // TODO: different VAO ctor to avoid needless large heap allocation 
                new uint[atomCapacity * indices.Count]); // TODO: different VAO ctor to avoid needless large heap allocation
            this.verticesPerAtom = verticesPerAtom;
            this.attributeGetters = attributeGetters;
            this.indices = indices;

            this.atomCapacity = atomCapacity;
        }

        /// <inheritdoc />
        public int Count => objects.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public void Add(T item)
        {
            // Check buffer size
            if (this.atomCount >= atomCapacity)
            {
                // TODO: expand (i.e. recreate) buffer? ResizeBuffer method in GlVertexArrayObject:
                // Create new, glCopyBufferSubData, delete old, update buffer ID array. 
                throw new InvalidOperationException("Buffer is full");
            }

            SetItemData(item, this.atomCount);

            objects.Add(item, this.atomCount);

            //TODO: item.PropertyChanged += Item_PropertyChanged;
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            throw new NotImplementedException();

            // TODO: Find buffer segment for object (will need to keep a record).
            // Overwrite it with last buffer segment.
            // Don't think need to do anything with indices because of their constant nature..
            objects.Remove(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            // TODO (if/when resizing is supported): clear buffer data / shrink buffer? 
            objects.Clear();
        }

        /// <inheritdoc />
        public bool Contains(T item) => objects.Keys.Contains(item);

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex) => objects.Keys.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => objects.Keys.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)objects).GetEnumerator();

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
            this.vao.Draw(this.atomCount * indices.Count);
        }

        /* TODO
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
            // TODO: more graceful error handling if the item not found
            var itemIndex = this.objects[item];
            SetItemData(item, itemIndex);
        }
        */

        /// <summary>
        /// Updates attribute buffers in the underlying VAO for a given item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="atomIndex">The index of the item within the underlying VAO.</param>
        private void SetItemData(T item, int atomIndex)
        {
            var atomCount = 0;
            for (int i = 0; i < this.attributeGetters.Count; i++)
            {
                var vertices = this.attributeGetters[i](item);

                if (vertices.Count % this.verticesPerAtom != 0)
                {
                    // TODO: and should be the same for each attribute getter..?
                    throw new InvalidOperationException();
                }

                atomCount = vertices.Count / verticesPerAtom;

                for (int j = 0; j < vertices.Count; j++)
                {
                    this.vao.Buffers[i].SetSubData(
                        atomIndex * this.verticesPerAtom + j,
                        vertices[j]);
                }
            }

            this.atomCount += atomCount;

            // Update the index
            for (int i = 0; i < this.indices.Count; i++)
            {
                vao.SetIndexData(
                    atomIndex * this.indices.Count + i,
                    (uint)(atomIndex * this.verticesPerAtom + this.indices[i]));
            }
        }
    }
}