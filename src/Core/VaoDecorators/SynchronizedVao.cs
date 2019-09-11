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
    public sealed class SynchronizedVao : IVertexArrayObject
    {
        private readonly IVertexArrayObject vertexArrayObject;
        private readonly SynchronizedVertexBufferObject indexBuffer;
        private readonly SynchronizedVertexBufferObject[] attributeBuffers;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedVao"/> class.
        /// </summary>
        /// <param name="vertexArrayObject"></param>
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
            vertexArrayObject?.Dispose();
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

            public object this[int index]
            {
                set => actions.Enqueue(() => vertexBufferObject[index] = value);
            }

            public uint Id => vertexBufferObject.Id;

            public GlVertexAttributeInfo[] Attributes => vertexBufferObject.Attributes;

            public int Capacity => vertexBufferObject.Capacity;

            public void Copy<T>(int readIndex, int writeIndex, int count)
            {
                actions.Enqueue(() => vertexBufferObject.Copy<T>(readIndex, writeIndex, count));
            }

            public void Dispose()
            {
                // No action: the inner VAO still owns the individual buffers
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
