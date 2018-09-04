namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// A OpenGL vertex buffer object. This class will map appropriately from .NET 
    /// </summary>
    public sealed class GlVertexBufferObject : IVertexBufferObject
    {
        private int count;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlVertexBufferObject"/> class. SIDE EFFECT: New buffer will be bound to the given target.
        /// </summary>
        /// <param name="target">OpenGL buffer target specification.</param>
        /// <param name="usage">OpenGL buffer usage specification.</param>
        /// <param name="vertexData">The vertex data to populate the buffer with. The data type must be a blittable value type (or an exception will be thrown).</param>
        public GlVertexBufferObject(BufferTarget target, BufferUsage usage, Array vertexData)
        {
            var elementType = vertexData.GetType().GetElementType();
            this.Attributes = GlVertexAttribInfo.ForType(elementType);
            this.count = vertexData.Length;

            this.Id = Gl.GenBuffer();
            Gl.BindBuffer(target, this.Id); // NB: Side effect - leaves this buffer bound. 
            Gl.BufferData(target, (uint)(Marshal.SizeOf(elementType) * vertexData.Length), vertexData, usage);
        }

        ~GlVertexBufferObject()
        {
            Gl.DeleteBuffers(this.Id);
        }

        /// <inheritdoc />
        public uint Id { get; private set; }

        /// <inheritdoc />
        public GlVertexAttribInfo[] Attributes { get; private set; }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                return count;
            }
            
            //set
            //{
            //    var newId = Gl.GenBuffer();
            //    Gl.NamedBufferData(newId, (uint)(Marshal.SizeOf(elementType) * value), null, usage);
            //    Gl.CopyNamedBufferSubData(this.Id, newId, 0, 0, (uint)(Marshal.SizeOf(elementType) * count));
            //    count = value;
            //}
        }

        /// <inheritdoc />
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
