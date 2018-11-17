namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// Encapsulates an OpenGL buffer bound to a set of <see cref="INotifyPropertyChanged"/> objects.
    /// </summary>
    /// <typeparam name="TItem">The type of objects.</typeparam>
    /// <typeparam name="TVertex">The type of vertex data to be stored in the buffer.</typeparam>
    public sealed class BoundBuffer<TItem, TVertex> where TItem : INotifyPropertyChanged
    {
        private readonly int verticesPerAtom;
        private readonly Func<TItem, IList<TVertex>> attributeGetter;
        private readonly IList<int> indices;
        private readonly IVertexArrayObject vao;
        private readonly List<Link> linksByBufferIndex = new List<Link>();

        private int atomCapacity;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundBuffer"/> class.
        /// </summary>
        /// <param name="collection">The collection to bind to.</param>
        /// <param name="primitiveType">The type of primitive to be drawn.</param>
        /// <param name="atomCapacity">The capacity for the buffer, in atoms.</param>
        /// <param name="vertexGetter">Delegate to transform source object into vertex data.</param>
        /// <param name="indices"></param>
        public BoundBuffer(
            PrimitiveType primitiveType,
            int atomCapacity,
            Func<TItem, IList<TVertex>> vertexGetter,
            IList<int> indices)
            : this(primitiveType, atomCapacity, vertexGetter, indices, GlVertexArrayObject.MakeVertexArrayObject)
        {
        }

        internal BoundBuffer(
            PrimitiveType primitiveType,
            int atomCapacity,
            Func<TItem, IList<TVertex>> vertexGetter,
            IList<int> indices,
            Func<PrimitiveType, IList<Tuple<BufferUsage, Array>>, uint[], IVertexArrayObject> makeVertexArrayObject)
        {
            this.verticesPerAtom = indices.Max() + 1; // TODO: throw if has unused indices?
            this.attributeGetter = vertexGetter;
            this.indices = indices;
            this.atomCapacity = atomCapacity;
            this.vao = makeVertexArrayObject(
                primitiveType,
                new[] { Tuple.Create(BufferUsage.DynamicDraw, Array.CreateInstance(typeof(TVertex), atomCapacity * verticesPerAtom)) },  // TODO: different VAO ctor to avoid needless large heap allocation 
                new uint[atomCapacity * indices.Count]); // TODO: different VAO ctor to avoid needless large heap allocation
        }
        
        /// <summary>
        /// Adds an item to the buffer.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>An object representing the appended buffer content.</returns>
        public Link Add(TItem item)
        {
           return new Link(this, item);
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
            this.vao.IndexBuffer.Flush();
            this.vao.AttributeBuffers[0].Flush();
            this.vao.Draw(linksByBufferIndex.Count * indices.Count);
        }

        public class Link
        {
            private readonly BoundBuffer<TItem, TVertex> parent;
            private SortedList<int, int> bufferIndices = new SortedList<int, int>();
            private TItem item; // Wouldn't be needed if Delete could accept item as a param (= if collection clear gave us the old items)..

            internal Link(BoundBuffer<TItem, TVertex> parent, TItem item)
            {
                this.parent = parent;
                SetItem(item);
            }

            public void ReplaceItem(TItem item)
            {
                this.item.PropertyChanged -= ItemPropertyChanged;
                SetItem(item);
            }

            public void Delete()
            {
                // TODO (if/when resizing is supported): clear buffer data / shrink buffer?
                this.item.PropertyChanged -= ItemPropertyChanged;
                while (bufferIndices.Count > 0)
                {
                    DeleteAtom(0);
                }
            }

            private void SetItem(TItem item)
            {
                this.item = item;
                this.SetBufferContent();
                item.PropertyChanged += ItemPropertyChanged;
            }

            private void ItemPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
            {
                this.item = (TItem)sender; // Needed if TItem is a value type
                this.SetBufferContent();
            }

            private void SetBufferContent()
            {
                var vertices = parent.attributeGetter(item);
                if (vertices.Count % parent.verticesPerAtom != 0)
                {
                    throw new InvalidOperationException($"Attribute getter must return multiple of correct number of vertices ({parent.verticesPerAtom}), but actually returned {vertices.Count}.");
                }

                var atomIndex = 0;
                for (; atomIndex < vertices.Count / parent.verticesPerAtom; atomIndex++)
                {
                    // Add a buffer index to the list if we need to
                    if (atomIndex >= bufferIndices.Count)
                    {
                        if (this.parent.linksByBufferIndex.Count >= this.parent.atomCapacity)
                        {
                            throw new InvalidOperationException("Buffer is full");
                        }

                        bufferIndices.Add(this.parent.linksByBufferIndex.Count, this.parent.linksByBufferIndex.Count);
                        this.parent.linksByBufferIndex.Add(this);
                    }

                    // Establish buffer index to write to
                    var bufferIndex = bufferIndices.Values[atomIndex];

                    // Set vertex attributes
                    for (int i = 0; i < parent.verticesPerAtom; i++)
                    {
                        parent.vao.AttributeBuffers[0][bufferIndex * parent.verticesPerAtom + i] = vertices[atomIndex * parent.verticesPerAtom + i];
                    }

                    // Update the index
                    for (int i = 0; i < parent.indices.Count; i++)
                    {
                        parent.vao.IndexBuffer[bufferIndex * parent.indices.Count + i] =
                            (uint)(bufferIndex * parent.verticesPerAtom + parent.indices[i]);
                    }
                }               
                while (atomIndex < bufferIndices.Count)
                {
                    DeleteAtom(atomIndex);
                }
            }

            private void DeleteAtom(int atomIndex)
            {
                var index = bufferIndices.Values[atomIndex];

                // Grab the last link by buffer index, remove its last 
                var finalBufferIndex = this.parent.linksByBufferIndex.Count - 1;
                var lastLink = this.parent.linksByBufferIndex[finalBufferIndex];
                lastLink.bufferIndices.RemoveAt(lastLink.bufferIndices.Count - 1);
                parent.linksByBufferIndex.RemoveAt(finalBufferIndex);

                // If last one in the buffer isn't the one being removed, move it
                // to replace the one being removed so that the buffer stays contiguous
                if (finalBufferIndex != index)
                {
                    this.bufferIndices.Remove(index);
                    lastLink.bufferIndices.Add(index, index);
                    this.parent.vao.AttributeBuffers[0].Copy<TVertex>(
                        finalBufferIndex * parent.verticesPerAtom,
                        index * parent.verticesPerAtom,
                        this.parent.verticesPerAtom);
                    parent.linksByBufferIndex[index] = lastLink;
                }
            }
        }
    }
}
