using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GLHDN.Core.VaoDecorators
{
    /// <summary>
    /// Decorator for <see cref="IVertexArrayObject"/> that synchronizes updates - flushing all pending changes when <see cref="Draw"/> is called.
    /// </summary>
    /// <remarks>
    /// TODO: this is a hacky, slow way to co-ordinate. Look into streaming.
    /// </remarks>
    public sealed class SynchronizedVao : IVertexArrayObject, IDisposable
    {
        private readonly IVertexArrayObject vertexArrayObject;
        private readonly SynchronizedVertexBufferObject indexBuffer;
        private readonly SynchronizedVertexBufferObject[] attributeBuffers;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedVao"/> class.
        /// </summary>
        /// <param name="vertexArrayObject">The VAO to apply synchromization to.</param>
        public SynchronizedVao(IVertexArrayObject vertexArrayObject)
        {
            this.vertexArrayObject = vertexArrayObject;
            this.indexBuffer = new SynchronizedVertexBufferObject(vertexArrayObject.IndexBuffer);
            this.attributeBuffers = new SynchronizedVertexBufferObject[vertexArrayObject.AttributeBuffers.Count];
            for (int i = 0; i < vertexArrayObject.AttributeBuffers.Count; i++)
            {
                this.attributeBuffers[i] = new SynchronizedVertexBufferObject(vertexArrayObject.AttributeBuffers[i]);
            }
        }

        /// <inheritdoc />
        public IVertexBufferObject IndexBuffer => indexBuffer;

        /// <inheritdoc />
        public IReadOnlyList<IVertexBufferObject> AttributeBuffers => attributeBuffers;

        /// <inheritdoc />
        public void Dispose()
        {
            // TODO LIFETIME MGMT: aggregated, not component - thus ideally not our responsibility
            // to Dispose (and thus no need for this class to be IDisposable)
            if (vertexArrayObject is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <inheritdoc />
        public void Draw(int count)
        {
            indexBuffer.Flush();
            for (int i = 0; i < attributeBuffers.Length; i++)
            {
                attributeBuffers[i].Flush();
            }

            vertexArrayObject.Draw(count);
        }

        private class SynchronizedVertexBufferObject : IVertexBufferObject
        {
            private readonly IVertexBufferObject vertexBufferObject;
            private readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

            public SynchronizedVertexBufferObject(IVertexBufferObject vertexBufferObject)
            {
                this.vertexBufferObject = vertexBufferObject;
            }

            public uint Id => vertexBufferObject.Id;

            public GlVertexAttributeInfo[] Attributes => vertexBufferObject.Attributes;

            public int Capacity => vertexBufferObject.Capacity;

            public object this[int index]
            {
                set => actions.Enqueue(() => vertexBufferObject[index] = value);
            }

            public void Copy<T>(int readIndex, int writeIndex, int count)
            {
                actions.Enqueue(() => vertexBufferObject.Copy<T>(readIndex, writeIndex, count));
            }

            /// <summary>
            /// Flush any changes to the underlying buffer.
            /// </summary>
            public void Flush()
            {
                // Only process the actions in the queue at the outset in case they are being continually added.
                for (int i = actions.Count; i > 0; i--)
                {
                    actions.TryDequeue(out var action);
                    action?.Invoke();
                }
            }

            public T Get<T>(int index)
            {
                return vertexBufferObject.Get<T>(index);
            }
        }
    }
}
