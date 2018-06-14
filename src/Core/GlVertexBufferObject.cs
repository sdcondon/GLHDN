namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Runtime.InteropServices;

    public sealed class GlVertexBufferObject
    {
        public GlVertexBufferObject(BufferTarget target, BufferUsage usage, Array data)
        {
            var elementType = data.GetType().GetElementType();
            this.Attributes = VertexAttribInfo.ForType(elementType);
            this.VertexCount = data.Length;

            this.Id = Gl.GenBuffer();
            Gl.BindBuffer(target, this.Id);
            Gl.BufferData(target, (uint)(Marshal.SizeOf(elementType) * data.Length), data, usage);
        }

        ~GlVertexBufferObject()
        {
            Gl.DeleteBuffers(this.Id);
        }

        public uint Id { get; private set; }

        public VertexAttribInfo[] Attributes { get; private set; }

        public int VertexCount { get; private set; }

        public object this[int offset]
        {
            set
            {
                Gl.NamedBufferSubData(
                    buffer: Id,
                    offset: new IntPtr(offset * Marshal.SizeOf(value)),
                    size: (uint)Marshal.SizeOf(value),
                    data: value);
            }
        }

        public void Dispose()
        {
            Gl.DeleteBuffers(this.Id);
            GC.SuppressFinalize(this);
        }
    }
}
