namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class MockVertexArrayObject : IVertexArrayObject
    {
        public MockVertexArrayObject(
            PrimitiveType primitiveType,
            IList<Tuple<BufferUsage, Array>> attributeBufferSpecs,
            uint[] indexData)
        {
            AttributeBuffers = attributeBufferSpecs.Select(a => new MockVertexBufferObject()).ToArray();
        }

        IVertexBufferObject IVertexArrayObject.IndexBuffer => IndexBuffer;

        public MockVertexBufferObject IndexBuffer { get; private set; } = new MockVertexBufferObject();

        IReadOnlyList<IVertexBufferObject> IVertexArrayObject.AttributeBuffers => AttributeBuffers;

        public IReadOnlyList<MockVertexBufferObject> AttributeBuffers { get; private set; }

        public void Dispose()
        {
        }

        public void Draw(int count)
        {
            throw new NotImplementedException();
        }
    }
}
