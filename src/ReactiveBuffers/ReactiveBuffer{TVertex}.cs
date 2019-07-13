namespace GLHDN.ReactiveBuffers
{
    using GLHDN.Core;
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Logic for creating and managing an Open GL buffer for a (observable) set of (observable) items that change over time,
    /// each of which provide a list of vertices to the buffer.
    /// </summary>
    /// <typeparam name="TVertex">The vertex data type.</typeparam>
    /// <remarks>
    /// TODO: probably best to remove the buffer creation stuff from here and turn this into an IObserver.
    /// </remarks>
    public class ReactiveBuffer<TVertex> : IDisposable
    {
        private readonly IObservable<IObservable<IList<TVertex>>> vertexSource;
        private readonly int verticesPerAtom;
        private readonly IList<int> indices;
        private readonly IVertexArrayObject vao;
        private readonly List<ItemObserver> linksByBufferIndex = new List<ItemObserver>();

        private int atomCapacity;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactiveBuffer{TVertex}"/> class.
        /// </summary>
        /// <param name="vertexSource">
        /// The outer observable should push a new inner observable whenever a new item is added.
        /// The inner observables should push a new list of vertices whenever the item's state changes, and complete when they are removed.
        /// </param>
        /// <param name="primitiveType">The type of primitive to be drawn.</param>
        /// <param name="atomCapacity">The capacity for the buffer, in atoms.</param>
        /// <param name="indices"></param>
        /// <param name="makeVertexArrayObject"></param>
        public ReactiveBuffer(
            IObservable<IObservable<IList<TVertex>>> vertexSource,
            PrimitiveType primitiveType,
            int atomCapacity,
            IList<int> indices,
            Func<PrimitiveType, IList<(BufferUsage, Type, int, Array)>, (int, uint[]), IVertexArrayObject> makeVertexArrayObject)
        {
            this.vertexSource = vertexSource;
            this.verticesPerAtom = indices.Max() + 1; // Perhaps should throw if has unused indices..
            this.indices = indices;  // TODO: Perhaps change so optional?
            this.atomCapacity = atomCapacity;
            this.vao = makeVertexArrayObject(
                primitiveType,
                new[] { (BufferUsage.DynamicDraw, typeof(TVertex), atomCapacity * verticesPerAtom, (Array)null) },
                (atomCapacity * indices.Count, null));

            // todo: store subscription to unsubscribe on dispose
            this.vertexSource.Subscribe(i => i.Subscribe(new ItemObserver(this)));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // todo: unsubscribe
            this.vao.Dispose();
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

        private class ItemObserver : IObserver<IList<TVertex>>
        {
            private readonly ReactiveBuffer<TVertex> parent;
            private SortedList<int, int> bufferIndices = new SortedList<int, int>();

            public ItemObserver(ReactiveBuffer<TVertex> parent)
            {
                this.parent = parent;
            }

            public void OnNext(IList<TVertex> vertices)
            {
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

            public void OnCompleted()
            {
                // TODO (if/when resizing is supported): clear buffer data / shrink buffer?
                while (bufferIndices.Count > 0)
                {
                    DeleteAtom(0);
                }
            }

            public void OnError(Exception error)
            {
                throw new AggregateException("Reactive buffer source errored", error);
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
