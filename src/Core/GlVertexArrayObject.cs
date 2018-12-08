namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an OpenGL vertex array object.
    /// </summary>
    public sealed class GlVertexArrayObject : IVertexArrayObject
    {
        private readonly uint id;
        private readonly PrimitiveType primitiveType;
        private readonly GlVertexBufferObject[] attributeBuffers;
        private readonly GlVertexBufferObject indexBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlVertexArrayObject"/> class. SIDE EFFECT: new VAO will be bound.
        /// </summary>
        /// <param name="primitiveType">OpenGL primitive type.</param>
        /// <param name="attributeBufferBuilders">Builder delegates for the buffers in this VAO.</param>
        /// <param name="indexData">The data to populate the index buffer with, or null if there should be no index.</param>
        internal GlVertexArrayObject(
            PrimitiveType primitiveType,
            IList<Tuple<BufferUsage, Array>> attributeBufferSpecs,
            uint[] indexData)
        {
            //  Record primitive type for use in draw calls, create and bind the VAO
            this.primitiveType = primitiveType;
            this.id = Gl.GenVertexArray(); // superbible uses CreateVertexArray?
            Gl.BindVertexArray(id);

            // Set up the attribute buffers
            this.attributeBuffers = new GlVertexBufferObject[attributeBufferSpecs.Count];
            uint k = 0;
            for (int i = 0; i < attributeBuffers.Length; i++)
            {
                var buffer = attributeBuffers[i] = new GlVertexBufferObject(BufferTarget.ArrayBuffer, attributeBufferSpecs[i].Item1, attributeBufferSpecs[i].Item2);
                for (uint j = 0; j < buffer.Attributes.Length; j++, k++)
                {
                    var attribute = buffer.Attributes[j];
                    Gl.EnableVertexAttribArray(k);
                    Gl.VertexAttribPointer(
                        index: k, // must match the layout in the shader
                        size: attribute.multiple,
                        type: attribute.type,
                        normalized: false,
                        stride: attribute.stride,
                        pointer: attribute.offset);
                }
            }

            // Establish element count & populate index buffer if there is one
            if (indexData != null)
            {
                this.indexBuffer = new GlVertexBufferObject(BufferTarget.ElementArrayBuffer, BufferUsage.DynamicDraw, indexData);
            }
        }

        /// <summary>
        /// Finalizer. Releases any unmanaged resources used by an object as it is GC'd.
        /// </summary>
        ~GlVertexArrayObject()
        {
            Dispose(false);
        }

        /// <summary>
        /// The number of vertices to be rendered.
        /// </summary>
        public int VertexCount => indexBuffer?.Count ?? attributeBuffers[0].Count;

        /// <inheritdoc />
        public IVertexBufferObject IndexBuffer => this.indexBuffer;

        /// <inheritdoc />
        public IReadOnlyList<IVertexBufferObject> AttributeBuffers => this.attributeBuffers;

        /// <inheritdoc />
        public void Draw(int count = -1)
        {
            Gl.BindVertexArray(this.id);

            // TODO: delegate instead of 'if' every time?
            if (indexBuffer != null)
            {
                // There's an index buffer (which will be bound) - bind it and draw
                Gl.DrawElements(
                    mode: this.primitiveType,
                    count: count == -1 ? this.VertexCount : count,
                    type: DrawElementsType.UnsignedInt,
                    indices: IntPtr.Zero);
            }
            else
            {
                // No index - so draw directly from index data
                Gl.DrawArrays(
                    mode: this.primitiveType,
                    first: 0,
                    count: count == -1 ? this.VertexCount : count);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /*public void ResizeAttributeBuffer(int bufferIndex, int newSize)
        {
            //var newId = Gl.GenBuffer();
            //Gl.NamedBufferData(newId, (uint)(Marshal.SizeOf(elementType) * value), null, usage);
            //Gl.CopyNamedBufferSubData(this.Id, newId, 0, 0, (uint)(Marshal.SizeOf(elementType) * count));
            //count = value;
        }*/

        public static IVertexArrayObject MakeVertexArrayObject(PrimitiveType primitiveType, IList<Tuple<BufferUsage, Array>> attributeBufferSpecs, uint[] indices)
        {
            return new GlVertexArrayObject(
                primitiveType,
                attributeBufferSpecs, // TODO: different VAO ctor to avoid needless large heap allocation 
                indices); // TODO: different VAO ctor to avoid needless large heap allocation
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var buffer in attributeBuffers)
                {
                    buffer.Dispose();
                }

                if (indexBuffer != null)
                {
                    indexBuffer.Dispose();
                }
            }

            Gl.DeleteVertexArrays(this.id);
        }
    }
}
