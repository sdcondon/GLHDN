namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <summary>
    /// Encapsulates an OpenGL buffer bound to a particular <see cref="INotifyCollectionChanged"/> object.
    /// </summary>
    /// <typeparam name="TItem">The type of objects in the collection object.</typeparam>
    /// <typeparam name="TVertex">The type of vertex data to e stored in the buffer.</typeparam>
    public sealed class CollectionBoundBuffer<TItem, TVertex> where TItem : INotifyPropertyChanged
    {
        private readonly BoundBuffer<TItem, TVertex> boundBuffer;
        private readonly List<BoundBuffer<TItem, TVertex>.Link> linksByCollectionIndex = new List<BoundBuffer<TItem, TVertex>.Link>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundBuffer"/> class.
        /// </summary>
        /// <param name="collection">The collection to bind to.</param>
        /// <param name="primitiveType">The type of primitive to be drawn.</param>
        /// <param name="atomCapacity">The capacity for the buffer, in atoms.</param>
        /// <param name="vertexGetter">Delegate to transform source object into vertex data.</param>
        /// <param name="indices"></param>
        public CollectionBoundBuffer(
            INotifyCollectionChanged collection,
            PrimitiveType primitiveType,
            int atomCapacity,
            Func<TItem, IList<TVertex>> vertexGetter,
            IList<int> indices)
            : this(collection, primitiveType, atomCapacity, vertexGetter, indices, GlVertexArrayObject.MakeVertexArrayObject)
        {
        }

        internal CollectionBoundBuffer(
            INotifyCollectionChanged collection,
            PrimitiveType primitiveType,
            int atomCapacity,
            Func<TItem, IList<TVertex>> vertexGetter,
            IList<int> indices,
            Func<PrimitiveType, IList<Tuple<BufferUsage, Array>>, uint[], IVertexArrayObject> makeVertexArrayObject)
        {
            this.boundBuffer = new BoundBuffer<TItem, TVertex>(primitiveType, atomCapacity, vertexGetter, indices, makeVertexArrayObject);
            collection.CollectionChanged += Collection_CollectionChanged;
        }

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

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        linksByCollectionIndex.Insert(e.NewStartingIndex + i, boundBuffer.Add((TItem)e.NewItems[i]));
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException();
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        linksByCollectionIndex[e.OldStartingIndex].Delete(); // not + i because we've already removed the preceding ones..
                        linksByCollectionIndex.RemoveAt(e.OldStartingIndex); // PERF: Slow..
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        linksByCollectionIndex[e.NewStartingIndex + i].ReplaceItem((TItem)e.NewItems[i]);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var link in linksByCollectionIndex)
                    {
                        link.Delete();
                    }
                    linksByCollectionIndex.Clear();
                    break;
            }
        }
    }
}
