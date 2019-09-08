namespace GLHDN.Core
{
    using System;

    /// <summary>
    /// Interface for types representing an OpenGL vertex buffer object.
    /// </summary>
    public interface IVertexBufferObject : IDisposable
    {
        /// <summary>
        /// Gets the ID of the buffer object.
        /// </summary>
        uint Id { get; }

        /// <summary>
        /// Gets the vertex attribute info for this buffer.
        /// </summary>
        GlVertexAttributeInfo[] Attributes { get; }

        /// <summary>
        /// Gets the number of vertices that the buffer has the capacity for.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Sets data for the vertex at a particular index.
        /// </summary>
        /// <param name="index">The index of the object to set.</param>
        object this[int index] { set; }

        /// <summary>
        /// Copy data internally within the buffer.
        /// </summary>
        /// <typeparam name="T">The type of object to treat the buffer content as.</typeparam>
        /// <param name="readIndex">The (object) index to read from.</param>
        /// <param name="writeIndex">The (object) index to write to.</param>
        /// <param name="count">The number of objects to copy.</param>
        void Copy<T>(int readIndex, int writeIndex, int count);

        /// <summary>
        /// Retrieves data from the buffer.
        /// </summary>
        /// <typeparam name="T">The type to interpret the data as.</typeparam>
        /// <param name="index">The object index to retrieve.</param>
        /// <returns>The buffer content at the requested index.</returns>
        T Get<T>(int index);
    }
}
