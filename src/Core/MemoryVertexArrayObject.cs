namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Implementation of <see cref="IVertexArrayObject"/> that just stores buffer content in memory, for testing purposes.
    /// </summary>
    public class MemoryVertexArrayObject : IVertexArrayObject
    {
        public MemoryVertexArrayObject(
            PrimitiveType primitiveType,
            IList<(BufferUsage, Array)> attributeBufferSpecs,
            uint[] indexData)
        {
            AttributeBuffers = attributeBufferSpecs.Select(a => new MemoryVertexBufferObject()).ToArray();
        }

        /// <inheritdoc />
        IVertexBufferObject IVertexArrayObject.IndexBuffer => IndexBuffer;

        /// <summary>
        /// Gets the <see cref="MemoryVertexBufferObject"/> that serves as the index buffer for this VAO.
        /// </summary>
        public MemoryVertexBufferObject IndexBuffer { get; private set; } = new MemoryVertexBufferObject();

        /// <inheritdoc />
        IReadOnlyList<IVertexBufferObject> IVertexArrayObject.AttributeBuffers => AttributeBuffers;

        /// <summary>
        ///  Gets the list of <see cref="MemoryVertexBufferObject"/> instances that serve as the attribute buffers for this VAO.
        /// </summary>
        public IReadOnlyList<MemoryVertexBufferObject> AttributeBuffers { get; private set; }

        /// <inheritdoc />
        public void Draw(int count)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
