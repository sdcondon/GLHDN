using System;

namespace GLHDN.Core
{
    public interface IVertexBufferObject : IDisposable
    {
        /// <summary>
        /// Gets the ID of the buffer object.
        /// </summary>
        uint Id { get; }

        /// <summary>
        /// Gets the vertex attribute info for this buffer.
        /// </summary>
        GlVertexAttribInfo[] Attributes { get; }

        /// <summary>
        /// Gets the number of vertices that the buffer contains data for.
        /// </summary>
        int VertexCount { get; }

        /// <summary>
        /// Sets data for the vertex at a particular index.
        /// </summary>
        /// <param name="index"></param>
        object this[int index] { set; }
    }
}
