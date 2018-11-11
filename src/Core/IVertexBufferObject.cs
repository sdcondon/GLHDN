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
        /// Gets the number of vertices that the buffer has the capacity for.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Sets data for the vertex at a particular index.
        /// </summary>
        /// <param name="index"></param>
        object this[int index] { set; }

        void Copy<T>(int readIndex, int writeIndex, int count);

        T Get<T>(int index);

        void Flush();
    }
}
