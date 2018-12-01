namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Implementation of <see cref="IVertexArrayObject"/> that just stores buffer content in memory, for testing purposes.
    /// </summary>
    public class MockVertexArrayObject : IVertexArrayObject
    {
        public MockVertexArrayObject(
            PrimitiveType primitiveType,
            IList<Tuple<BufferUsage, Array>> attributeBufferSpecs,
            uint[] indexData)
        {
            AttributeBuffers = attributeBufferSpecs.Select(a => new MockVertexBufferObject()).ToArray();
        }

        /// <inheritdoc />
        IVertexBufferObject IVertexArrayObject.IndexBuffer => IndexBuffer;

        public MockVertexBufferObject IndexBuffer { get; private set; } = new MockVertexBufferObject();

        /// <inheritdoc />
        IReadOnlyList<IVertexBufferObject> IVertexArrayObject.AttributeBuffers => AttributeBuffers;

        public IReadOnlyList<MockVertexBufferObject> AttributeBuffers { get; private set; }

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
