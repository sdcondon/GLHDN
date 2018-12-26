namespace GLHDN.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for types representing an OpenGL vertex array object.
    /// </summary>
    public interface IVertexArrayObject : IDisposable
    {
        /// <summary>
        /// Gets the index buffer object for this VAO, if there is one.
        /// </summary>
        IVertexBufferObject IndexBuffer { get; }

        /// <summary>
        /// Gets the set of buffer objects contained within this VAO.
        /// </summary>
        IReadOnlyList<IVertexBufferObject> AttributeBuffers { get; }

        /// <summary>
        /// Draw with the active program.
        /// </summary>
        /// <param name="count">The number of vertices to draw.</param>
        void Draw(int count);
    }
}
