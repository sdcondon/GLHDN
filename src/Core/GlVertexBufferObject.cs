namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Runtime.InteropServices;

    public sealed class GlVertexBufferObject
    {
        public GlVertexBufferObject(BufferUsage usage, Array data)
        {
            var elementType = data.GetType().GetElementType();
            this.Attributes = VertexAttribInfo.ForType(elementType);
            this.VertexCount = data.Length;

            this.Id = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, this.Id);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Marshal.SizeOf(elementType) * data.Length), data, usage);
        }

        ~GlVertexBufferObject()
        {
            Gl.DeleteBuffers(this.Id);
        }

        public uint Id { get; private set; }

        public VertexAttribInfo[] Attributes { get; private set; }

        public int VertexCount { get; private set; }

        public void SetSubData(int offset, object data)
        {
            Gl.NamedBufferSubData(
                buffer: Id,
                offset: new IntPtr(offset * Marshal.SizeOf(data)),
                size: (uint)Marshal.SizeOf(data),
                data: data);
        }

        public void Dispose()
        {
            Gl.DeleteBuffers(this.Id);
            GC.SuppressFinalize(this);
        }
    }
}
