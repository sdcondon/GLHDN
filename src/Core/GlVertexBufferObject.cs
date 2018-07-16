namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// A OpenGL vertex buffer object. This class will map appropriately from .NET 
    /// </summary>
    public sealed class GlVertexBufferObject : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GlVertexBufferObject"/> class.
        /// </summary>
        /// <param name="target">OpenGL buffer target specification.</param>
        /// <param name="usage">OpenGL buffer usage specification.</param>
        /// <param name="vertexData">The vertex data to populate the buffer with. The data type must be a blittable value type (or an exception will be thrown).</param>
        public GlVertexBufferObject(BufferTarget target, BufferUsage usage, Array vertexData)
        {
            var elementType = vertexData.GetType().GetElementType();
            this.Attributes = GlVertexAttribInfo.ForType(elementType);
            this.VertexCount = vertexData.Length;

            this.Id = Gl.GenBuffer();
            Gl.BindBuffer(target, this.Id); // NB: Side effect - leaves this buffer bound. 
            Gl.BufferData(target, (uint)(Marshal.SizeOf(elementType) * vertexData.Length), vertexData, usage);
        }

        ~GlVertexBufferObject()
        {
            Gl.DeleteBuffers(this.Id);
        }

        /// <summary>
        /// Gets the ID of the buffer object.
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// Gets the vertex attribute info for this buffer.
        /// </summary>
        public GlVertexAttribInfo[] Attributes { get; private set; }

        /// <summary>
        /// Gets the number of vertices that the buffer contains data for.
        /// </summary>
        public int VertexCount { get; private set; }

        /// <summary>
        /// Sets data for the vertex at a particular index.
        /// </summary>
        /// <param name="index"></param>
        public object this[int index]
        {
            set
            {
                Gl.NamedBufferSubData(
                    buffer: Id,
                    offset: new IntPtr(index * Marshal.SizeOf(value)),
                    size: (uint)Marshal.SizeOf(value),
                    data: value);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Gl.DeleteBuffers(this.Id);
            GC.SuppressFinalize(this);
        }
    }
}
