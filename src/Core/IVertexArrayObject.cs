namespace GLHDN.Core
{
    using System;
    using System.Collections.Generic;

    /// <remarks>
    /// For testability.
    /// </remarks>
    interface IVertexArrayObject : IDisposable
    {
        /// <summary>
        /// Gets the index buffer object for this VAO, if there is one.
        /// </summary>
        GlVertexBufferObject IndexBuffer { get; }

        /// <summary>
        /// Gets the set of buffer objects contained within this VAO.
        /// </summary>
        IReadOnlyList<GlVertexBufferObject> AttributeBuffers { get; }

        /// <summary>
        /// Draw with the active program.
        /// </summary>
        /// <param name="count"></param>
        void Draw(int count);
    }
}
