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
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryVertexArrayObject"/> class.
        /// </summary>
        /// <param name="primitiveType">OpenGL primitive type.</param>
        /// <param name="attributeBufferSpecs">Specs for the buffers in this VAO.</param>
        /// <param name="indexSpec">The data to populate the index buffer with, or null if there should be no index.</param>
        public MemoryVertexArrayObject(
            PrimitiveType primitiveType,
            IList<(BufferUsage, Type, int, Array)> attributeBufferSpecs,
            (int, uint[]) indexSpec)
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
