namespace GLHDN.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    class MockVertexBufferObject : IVertexBufferObject
    {
        private static int nextId = 0;

        public MockVertexBufferObject()
        {
            Id = (uint)Interlocked.Increment(ref nextId);
        }

        public uint Id { get; private set; }

        public List<object> Contents { get; private set; } = new List<object>();

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

        public GlVertexAttribInfo[] Attributes => throw new NotImplementedException();

        public int VertexCount => Contents.Count;

        public void Dispose()
        {
        }
    }
}
