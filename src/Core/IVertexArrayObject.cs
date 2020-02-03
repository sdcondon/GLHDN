using System.Collections.Generic;

namespace GLHDN.Core
{
    /// <summary>
    /// Interface for types representing an OpenGL vertex array object.
    /// </summary>
    public interface IVertexArrayObject
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
        /// <param name="count">The number of vertices to draw, or -1 to draw all vertices.</param>
        void Draw(int count);
    }
}
