namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an OpenGL vertex array object.
    /// </summary>
    public sealed class GlVertexArrayObject : IDisposable
    {
        private readonly uint id;
        private readonly PrimitiveType primitiveType;
        private readonly GlVertexBufferObject[] attributeBuffers;
        private readonly uint? indexBufferId;
        private readonly int vertexCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlVertexArrayObject"/> class.
        /// </summary>
        /// <param name="primitiveType">OpenGL primitive type.</param>
        /// <param name="attributeBufferBuilders">Builder delegates for the buffers in this VAO.</param>
        /// <param name="indexData"></param>
        internal GlVertexArrayObject(
            PrimitiveType primitiveType,
            IList<Func<GlVertexBufferObject>> attributeBufferBuilders,
            uint[] indexData)
        {
            //  Record primitive type for use in draw calls, create and bind the VAO
            this.primitiveType = primitiveType;
            this.id = Gl.GenVertexArray(); // TODO: superbible uses CreateVertexArray?
            Gl.BindVertexArray(id);

            // Set up the attribute buffers
            this.attributeBuffers = new GlVertexBufferObject[attributeBufferBuilders.Count];
            uint k = 0;
            for (int i = 0; i < attributeBuffers.Length; i++)
            {
                var buffer = attributeBuffers[i] = attributeBufferBuilders[i](); // TODO: Assert length consistency?
                Gl.BindBuffer(BufferTarget.ArrayBuffer, buffer.Id);
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
                this.indexBufferId = Gl.GenBuffer();
                Gl.BindBuffer(BufferTarget.ElementArrayBuffer, this.indexBufferId.Value); // NB: important to bind this last
                Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(sizeof(uint) * indexData.Length), indexData, BufferUsage.DynamicDraw);
                this.vertexCount = indexData.Length;
            }
            else
            {
                this.vertexCount = attributeBuffers[0].VertexCount;
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
        /// Gets the set of buffer objects contained within this VAO.
        /// </summary>
        public IReadOnlyList<GlVertexBufferObject> Buffers => this.attributeBuffers;

        /// <summary>
        /// Draw with the active program.
        /// </summary>
        public void Draw(int count = -1)
        {
            Gl.BindVertexArray(this.id);

            // TODO: delegate instead of 'if' every time?
            if (indexBufferId.HasValue)
            {
                // There's an index buffer (which will be bound) - bind it and draw
                Gl.DrawElements(
                    mode: this.primitiveType,
                    count: count == -1 ? this.vertexCount : count,
                    type: DrawElementsType.UnsignedInt,
                    indices: IntPtr.Zero);
            }
            else
            {
                // No index - so draw directly from index data
                Gl.DrawArrays(
                    mode: this.primitiveType,
                    first: 0,
                    count: count == -1 ? this.vertexCount : count);
            }
        }

        public void SetIndexData(int offset, uint data)
        {
            Gl.NamedBufferSubData(
                this.indexBufferId.Value,
                new IntPtr(offset * sizeof(uint)),
                sizeof(uint),
                data);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var buffer in attributeBuffers)
                {
                    buffer.Dispose();
                }
            }

            Gl.DeleteVertexArrays(this.id);
        }
    }
}
