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
        /// <param name="attributeBufferSpecs">Specs for the buffers in this VAO.</param>
        /// <param name="indexSpec">Spec for the index of this VAO.</param>
        public MemoryVertexArrayObject(
            IList<(BufferUsage usage, Type elementType, int capacity, Array data)> attributeBufferSpecs,
            (int capacity, uint[] data) indexSpec)
        {
            AttributeBuffers = attributeBufferSpecs.Select(a => new MemoryVertexBufferObject(a.capacity, a.data)).ToArray();
            IndexBuffer = new MemoryVertexBufferObject(indexSpec.capacity, indexSpec.data);
        }

        /// <inheritdoc />
        IVertexBufferObject IVertexArrayObject.IndexBuffer => IndexBuffer;

        /// <summary>
        /// Gets the <see cref="MemoryVertexBufferObject"/> that serves as the index buffer for this VAO.
        /// </summary>
        public MemoryVertexBufferObject IndexBuffer { get; }

        /// <inheritdoc />
        IReadOnlyList<IVertexBufferObject> IVertexArrayObject.AttributeBuffers => AttributeBuffers;

        /// <summary>
        ///  Gets the list of <see cref="MemoryVertexBufferObject"/> instances that serve as the attribute buffers for this VAO.
        /// </summary>
        public IReadOnlyList<MemoryVertexBufferObject> AttributeBuffers { get; }

        /// <inheritdoc />
        public void Draw(int count)
        {
            throw new NotImplementedException();
        }
    }
}
