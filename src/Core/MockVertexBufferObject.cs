namespace GLHDN.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Implementation of <see cref="IVertexBufferObject"/> that just stores buffer content in memory, for testing purposes.
    /// </summary>
    public class MockVertexBufferObject : IVertexBufferObject
    {
        private static int nextId = 0;

        public MockVertexBufferObject()
        {
            Id = (uint)Interlocked.Increment(ref nextId);
        }

        public List<object> Contents { get; private set; } = new List<object>();

        /// <inheritdoc />
        public uint Id { get; private set; }

        /// <inheritdoc />
        public object this[int index]
        {
            get => Contents[index];
            set
            {
                while (index >= Contents.Count)
                {
                    Contents.Add(null);
                }
                Contents[index] = value;
            }
        }

        /// <inheritdoc />
        public void Copy<T>(int readIndex, int writeIndex, int count)
        {
            for (var i = 0; i < count; i++)
            {
                Contents[writeIndex + i] = Contents[readIndex + i];
            }
        }

        /// <inheritdoc />
        public T Get<T>(int index)
        {
            return (T)Contents[index];
        }

        /// <inheritdoc />
        public void Flush()
        {

        }

        /// <inheritdoc />
        public GlVertexAttribInfo[] Attributes => throw new NotImplementedException();

        /// <inheritdoc />
        public int Count => Contents.Count;

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
