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
            IList<Tuple<BufferUsage, Array>> attributeBufferSpecs,
            uint[] indexData)
        {
            AttributeBuffers = attributeBufferSpecs.Select(a => new MemoryVertexBufferObject()).ToArray();
        }

        /// <inheritdoc />
        IVertexBufferObject IVertexArrayObject.IndexBuffer => IndexBuffer;

        public MemoryVertexBufferObject IndexBuffer { get; private set; } = new MemoryVertexBufferObject();

        /// <inheritdoc />
        IReadOnlyList<IVertexBufferObject> IVertexArrayObject.AttributeBuffers => AttributeBuffers;

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
