namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// A OpenGL vertex buffer object.
    /// </summary>
    internal sealed class GlVertexBufferObject : IVertexBufferObject, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GlVertexBufferObject"/> class. SIDE EFFECT: New buffer will be bound to the given target.
        /// </summary>
        /// <param name="target">OpenGL buffer target specification.</param>
        /// <param name="usage">OpenGL buffer usage specification.</param>
        /// <param name="elementType">The type of elements to be stored in the buffer. The data type must be a blittable value type (or an exception will be thrown).</param>
        /// <param name="elementCapacity">The maximum number of elements to be stored in the buffer.</param>
        /// <param name="elementData">The data to populate the buffer with.</param>
        public GlVertexBufferObject(BufferTarget target, BufferUsage usage, Type elementType, int elementCapacity, Array elementData)
        {
            this.Attributes = GlVertexAttributeInfo.ForType(elementType);
            this.Capacity = elementCapacity;

            this.Id = Gl.GenBuffer();
            Gl.BindBuffer(target, this.Id); // NB: Side effect - leaves this buffer bound.
            Gl.BufferData(target, (uint)(Marshal.SizeOf(elementType) * elementCapacity), elementData, usage);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GlVertexBufferObject"/> class.
        /// </summary>
        ~GlVertexBufferObject() => Dispose(false);

        /// <inheritdoc />
        public uint Id { get; }

        /// <inheritdoc />
        public GlVertexAttributeInfo[] Attributes { get; }

        /// <inheritdoc />
        public int Capacity { get; }

        /// <inheritdoc />
        public object this[int index]
        {
            ////get
            ////{
            ////    Gl.GetNamedBufferSubData(
            ////        buffer: Id,
            ////        offset: new IntPtr(index * Marshal.SizeOf(value)),
            ////        size: (uint)Marshal.SizeOf(value),
            ////        data: null);
            ////}
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
        public void Copy<T>(int readIndex, int writeIndex, int count)
        {
            var elementSize = Marshal.SizeOf(typeof(T));
            Gl.CopyNamedBufferSubData(
                readBuffer: Id,
                writeBuffer: Id,
                readOffset: new IntPtr(readIndex * elementSize),
                writeOffset: new IntPtr(writeIndex * elementSize),
                size: (uint)(count * elementSize));
        }

        /// <inheritdoc />
        public T Get<T>(int index)
        {
            var elementSize = Marshal.SizeOf(typeof(T));
            var ptr = Marshal.AllocHGlobal(elementSize);
            try
            {
                Gl.GetNamedBufferSubData(
                    buffer: this.Id,
                    offset: new IntPtr(index * elementSize),
                    size: (uint)elementSize,
                    data: ptr);
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool _)
        {
            if (DeviceContext.GetCurrentContext() != IntPtr.Zero)
            {
                Gl.DeleteBuffers(this.Id);
            }
        }
    }
}
