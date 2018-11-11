namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// Wraps a <see cref="GlVertexArrayObject"/> that stores vertex data for a collection of objects.
    /// Presents simply as an <see cref="ICollection{TItem}"/> of the objects.
    /// Updates the backing VAO appropriately as the collection (and the items within it) is modified.
    /// </summary>
    public class CollectionBuffer<TItem, TVertex> : ICollection<TItem>, IDisposable where TItem : INotifyPropertyChanged
    {
        private readonly BoundBuffer<TItem, TVertex> boundBuffer;
        private readonly Dictionary<TItem, BoundBuffer<TItem, TVertex>.Link> links = new Dictionary<TItem, BoundBuffer<TItem, TVertex>.Link>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundBuffer"/> class.
        /// </summary>
        /// <param name="collection">The collection to bind to.</param>
        /// <param name="primitiveType">The type of primitive to be drawn.</param>
        /// <param name="objectCapacity">The capacity for the buffer, in objects.</param>
        /// <param name="vertexGetter">Delegate to transform source object into vertex data.</param>
        /// <param name="indices"></param>
        public CollectionBuffer(
            PrimitiveType primitiveType,
            int objectCapacity,
            Func<TItem, IList<TVertex>> vertexGetter,
            IList<int> indices)
            : this(primitiveType, objectCapacity, vertexGetter, indices, GlVertexArrayObject.MakeVertexArrayObject)
        {
        }

        internal CollectionBuffer(
            PrimitiveType primitiveType,
            int atomCapacity,
            Func<TItem, IList<TVertex>> vertexGetter,
            IList<int> indices,
            Func<PrimitiveType, IList<Tuple<BufferUsage, Array>>, uint[], IVertexArrayObject> makeVertexArrayObject)
        {
            this.boundBuffer = new BoundBuffer<TItem, TVertex>(primitiveType, atomCapacity, vertexGetter, indices, makeVertexArrayObject);
        }

        /// <inheritdoc />
        public int Count => links.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public void Add(TItem item)
        {
            this.links[item] = this.boundBuffer.Add(item);
        }

        /// <inheritdoc />
        public bool Remove(TItem item)
        {
            if (this.links.TryGetValue(item, out var link))
            {
                link.Delete();
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void Clear()
        {
            foreach (var link in links.Values)
            {
                link.Delete();
            }
            links.Clear();
        }

        /// <inheritdoc />
        public bool Contains(TItem item) => links.Keys.Contains(item);

        /// <inheritdoc />
        public void CopyTo(TItem[] array, int arrayIndex) => links.Keys.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public IEnumerator<TItem> GetEnumerator() => links.Keys.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)links).GetEnumerator();

        /// <inheritdoc />
        public void Dispose()
        {
            this.boundBuffer.Dispose();
        }

        /// <summary>
        /// Draw with the active program.
        /// </summary>
        public void Draw()
        {
            this.boundBuffer.Draw();
        }
    }
}