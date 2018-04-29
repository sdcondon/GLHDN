namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Wraps a <see cref="GlVertexArrayObject"/> that stores vertex data for a collection of objects.
    /// Presents simply as an <see cref="ICollection{T}"/> of the objects.
    /// Updates the backing VAO appropriately as the collection is modified.
    /// </summary>
    public sealed class ObjectBuffer<T> : ICollection<T>, IDisposable
    {
        private readonly Dictionary<T, int> objects = new Dictionary<T, int>();
        private readonly GlVertexArrayObject vao;
        private readonly int verticesPerObject;
        private readonly IList<Func<T, int, object>> attributeGetters;
        private readonly IList<int> indices;

        private int objectCapacity;

        internal ObjectBuffer(
            PrimitiveType primitiveType,
            int verticesPerObject,
            int objectCapacity,
            IList<Type> attributeTypes,
            IList<Func<T, int, object>> attributeGetters,
            IList<int> indices)
        {
            this.vao = new GlVertexArrayObject(
                primitiveType,
                attributeTypes.Select(a => BufferUsage.DynamicDraw).ToArray(),
                attributeTypes.Select(a => Array.CreateInstance(a, objectCapacity * verticesPerObject)).ToArray(), // TODO: different VAO ctor to avoid needless large heap allocation 
                new uint[objectCapacity * indices.Count]); // TODO: different VAO ctor to avoid needless large heap allocation
            this.verticesPerObject = verticesPerObject;
            this.attributeGetters = attributeGetters;
            this.indices = indices;

            this.objectCapacity = objectCapacity;
        }

        /// <inheritdoc />
        public int Count => objects.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public void Add(T item)
        {
            var itemIndex = objects.Count;

            // Check buffer size
            if (itemIndex >= objectCapacity)
            {
                // TODO: expand (i.e. recreate) buffer? ResizeBuffer method in GlVertexArrayObject:
                // Create new, glCopyBufferSubData, delete old, update buffer ID array. 
                throw new InvalidOperationException("Buffer is full");
            }

            SetAttributeData(item, itemIndex);

            // Update the index
            for (int i = 0; i < this.indices.Count; i++)
            {
                vao.SetIndexData(
                    itemIndex * indices.Count + i,
                    (uint)(itemIndex * verticesPerObject + indices[i]));
            }

            objects.Add(item, itemIndex);
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
        public bool Remove(T item)
        {
            throw new NotImplementedException();

            // TODO: Find buffer segment for object (will need to keep a record).
            // Overwrite it with last buffer segment.
            // Don't think need to do anything with indices because of their constant nature..
            objects.Remove(item);
        }

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
            this.vao.Draw(objects.Count * indices.Count);
        }

        public void ItemChanged(T item)
        {
            // TODO: more graceful error handling if ege item not found
            var itemIndex = this.objects[item];
            SetAttributeData(item, itemIndex);
        }

        /// <summary>
        /// Updates attribute buffers in the underlying VAO for a given item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="itemIndex">The index of the item within the underlying VAO.</param>
        private void SetAttributeData(T item, int itemIndex)
        {
            for (int i = 0; i < this.attributeGetters.Count; i++)
            {
                for (int j = 0; j < this.verticesPerObject; j++)
                {
                    this.vao.BufferSubData(
                        i,
                        itemIndex * this.verticesPerObject + j,
                        this.attributeGetters[i](item, j));
                }
            }
        }
    }
}