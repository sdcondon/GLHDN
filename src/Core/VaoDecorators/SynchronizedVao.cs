using OpenGL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GLHDN.Core.VaoDecorators
{
    /// <summary>
    /// Decorator for <see cref="IVertexArrayObject"/> that explicitly synchronizes with OpenGL (making it simple but slow..).
    /// <para/>
    /// See https://www.khronos.org/opengl/wiki/Synchronization#Implicit_synchronization for some info.
    /// </summary>
    /// <remarks>
    /// TODO: Look into some alternative decorators that do e.g. streaming - https://www.khronos.org/opengl/wiki/Buffer_Object_Streaming.
    /// </remarks>
    public sealed class SynchronizedVao : IVertexArrayObject, IDisposable
    {
        private readonly IVertexArrayObject vertexArrayObject;
        private readonly SynchronizedVertexBufferObject indexBuffer;
        private readonly SynchronizedVertexBufferObject[] attributeBuffers;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedVao"/> class.
        /// </summary>
        /// <param name="vertexArrayObject">The VAO to apply synchronization to.</param>
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
            Gl.Finish();

            indexBuffer.FlushChanges();
            for (int i = 0; i < attributeBuffers.Length; i++)
            {
                attributeBuffers[i].FlushChanges();
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
            public void FlushChanges()
            {
                // Only process the actions in the queue at the outset in case they are being continually added.
                for (int i = actions.Count; i > 0; i--)
                {
                    actions.TryDequeue(out var action);
                    action?.Invoke();
                }
            }

            public T GetAs<T>(int index) => vertexBufferObject.GetAs<T>(index);
        }
    }
}
