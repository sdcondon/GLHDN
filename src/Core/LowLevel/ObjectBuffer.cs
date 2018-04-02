namespace OpenGlHelpers.Core
{
    using OpenGL;
    using OpenGlHelpers.Core.LowLevel;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Wraps a <see cref="GlVertexArrayObject"/> that stores vertex data for a collection of objects.
    /// Presents simply as an <see cref="ICollection{T}"/> of the objects.
    /// Useful for dynamic <see cref="IRenderable"/> implementations.
    /// </summary>
    public sealed class ObjectBuffer<T> : ICollection<T>, IDisposable
    {
        private readonly GlVertexArrayObject vao;
        private readonly int verticesPerObject;
        private readonly IList<Func<T, int, object>> attributeGetters;
        private readonly HashSet<T> objects = new HashSet<T>();

        private int objectCapacity = 0;

        public ObjectBuffer(
            PrimitiveType primitiveType,
            int verticesPerObject,
            int objectCapacity,
            IList<Type> attributeTypes,
            IList<Func<T, int, object>> attributeGetters)
        {
            this.vao = new GlVertexArrayObject(
                primitiveType,
                attributeTypes.Select(a => BufferUsage.DynamicDraw).ToArray(),
                attributeTypes.Select(a => Array.CreateInstance(a, objectCapacity * verticesPerObject)).ToArray(),
                null);
            this.verticesPerObject = verticesPerObject;
            this.objectCapacity = objectCapacity;
            this.attributeGetters = attributeGetters;
        }

        /// <inheritdoc />
        public int Count => objects.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public void Add(T item)
        {
            // Check buffer size
            if (objects.Count >= objectCapacity)
            {
                throw new InvalidOperationException("Buffer is full");
                // TODO: expand (i.e. recreate) buffer? ResizeBuffer method in GlVertexArrayObject:
                // Create new, glCopyBufferSubData, delete old, update buffer ID array. 
            }

            // Get attribute values and update the buffers
            for (int i = 0; i < attributeGetters.Count; i++)
            {
                for (int j = 0; j < verticesPerObject; j++)
                {
                    vao.BufferSubData(
                        i,
                        objects.Count * verticesPerObject + j,
                        attributeGetters[i](item, j));
                }
            }

            objects.Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            // TODO (if/when resizing is supported): clear buffer data / shrink buffer? 
            objects.Clear();
        }

        /// <inheritdoc />
        public bool Contains(T item) => objects.Contains(item);

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex) => objects.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => objects.GetEnumerator();

        /// <inheritdoc />
        public bool Remove(T item)
        {
            throw new NotImplementedException();

            // TODO: Find buffer segment for object (will need to keep a record), overwrite it with last buffer
            // segment. Shouldn't need an index shenanigans, since each object will have an unrelated set of vertices.
            objects.Remove(item);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)objects).GetEnumerator();

        /// <summary>
        /// Draw with the active program.
        /// </summary>
        public void Draw()
        {
            this.vao.Draw(objects.Count * verticesPerObject);
        }

        public void ItemChanged(T item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.vao.Dispose();
        }
    }
}