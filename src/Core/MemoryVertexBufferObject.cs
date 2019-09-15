namespace GLHDN.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Implementation of <see cref="IVertexBufferObject"/> that just stores buffer content in memory, for testing purposes.
    /// </summary>
    public class MemoryVertexBufferObject : IVertexBufferObject
    {
        private static int nextId = 0;
        private readonly object[] content;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryVertexBufferObject"/> class.
        /// </summary>
        /// <param name="capacity">The capacity of the buffer.</param>
        /// <param name="data">The data to populate the buffer with, or null.</param>
        public MemoryVertexBufferObject(int capacity, Array data)
        {
            Id = (uint)Interlocked.Increment(ref nextId);
            content = new object[capacity];
            data?.CopyTo(content, 0);
        }

        /// <summary>
        /// Gets the content of the buffer.
        /// </summary>
        public IReadOnlyList<object> Content => content;

        /// <inheritdoc />
        public uint Id { get; }

        /// <inheritdoc />
        public GlVertexAttributeInfo[] Attributes => throw new NotImplementedException();

        /// <inheritdoc />
        public int Capacity => Content.Count;

        /// <inheritdoc />
        public object this[int index]
        {
            get => content[index];
            set => content[index] = value;
        }

        /// <inheritdoc />
        public void Copy<T>(int readIndex, int writeIndex, int count) => Array.Copy(content, readIndex, content, writeIndex, count);

        /// <inheritdoc />
        public T Get<T>(int index) => (T)Content[index];
    }
}
