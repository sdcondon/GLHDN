namespace OpenGlHelpers.Core.LowLevel
{
    using OpenGL;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Numerics;

    /// <summary>
    /// Represents an Open GL vertex array object.
    /// </summary>
    public sealed class GlVertexArrayObject : IDisposable
    {
        private readonly uint id;
        private readonly PrimitiveType primitiveType;

        private readonly uint[] attributeBufferIds;
        private readonly VertexAttribType[] attributeBufferVertexAttribTypes;
        private readonly int[] attributeBufferVertexAttribMultiples;
        private readonly uint? indexBufferId;
        private readonly int vertexCount;

        internal GlVertexArrayObject(
            PrimitiveType primitiveType,
            IList<BufferUsage> attributeUsages,
            IList<ICollection> attributeData,
            uint[] indexData)
        {
            // Create and bind the VAO
            this.id = Gl.GenVertexArray();
            Gl.BindVertexArray(id);

            // Record primitive type for use in draw calls
            this.primitiveType = primitiveType;

            // Create and populate the attribute buffers
            var attributeCount = attributeUsages.Count; // TODO: Assert length consistency?
            this.attributeBufferIds = new uint[attributeCount];
            this.attributeBufferVertexAttribTypes = new VertexAttribType[attributeCount];
            this.attributeBufferVertexAttribMultiples = new int[attributeCount];
            Gl.GenBuffers(attributeBufferIds);
            for (int i = 0; i < attributeCount; i++)
            {
                Gl.BindBuffer(BufferTarget.ArrayBuffer, attributeBufferIds[i]);
                Gl.BufferData(BufferTarget.ArrayBuffer, GetBufferSize(attributeData[i]), attributeData[i], attributeUsages[i]);
                this.attributeBufferVertexAttribTypes[i] = GetVertexAttribType(attributeData[i].GetType());
                this.attributeBufferVertexAttribMultiples[i] = GetVertexAttribMultiple(attributeData[i].GetType());
            }

            // Establish element count & populate index buffer if there is one
            if (indexData != null)
            {
                this.indexBufferId = Gl.GenBuffer();
                Gl.BindBuffer(BufferTarget.ElementArrayBuffer, this.indexBufferId.Value);
                Gl.BufferData(BufferTarget.ElementArrayBuffer, GetBufferSize(indexData), indexData, BufferUsage.StaticDraw);
                this.vertexCount = indexData.Length;
            }
            else
            {
                this.vertexCount = attributeData[0].Count;
            }
        }

        /// <summary>
        /// Finalizer. Releases any unmanaged resources used by an object as it is GC'd.
        /// </summary>
        ~GlVertexArrayObject()
        {
            Gl.DeleteBuffers(this.attributeBufferIds); // TODO: fix me? - can't reliably reference ref type in finalizer.
            Gl.DeleteVertexArrays(this.id);
        }

        /// <summary>
        /// Draw with the active program. TODO: Allow specification of buffer binding?
        /// </summary>
        public void Draw(int count = -1)
        {
            Gl.BindVertexArray(this.id);

            // Set the attribute pointers..
            for (uint i = 0; i < attributeBufferIds.Length; i++)
            {
                Gl.EnableVertexAttribArray(i);
                Gl.BindBuffer(BufferTarget.ArrayBuffer, attributeBufferIds[i]);
                Gl.VertexAttribPointer(
                    index: i, // must match the layout in the shader
                    size: attributeBufferVertexAttribMultiples[i],
                    type: attributeBufferVertexAttribTypes[i],
                    normalized: false,
                    stride: 0,
                    pointer: IntPtr.Zero);
            }

            // ..and draw
            // TODO: delegate instead of 'if' every time?
            if (indexBufferId.HasValue)
            {
                // There's an index buffer - bind it and draw
                Gl.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId.Value);
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

            // Tidy up
            for (uint i = 0; i < attributeBufferIds.Length; i++)
            {
                Gl.DisableVertexAttribArray(i);
            }
        }

        public void BufferSubData(int bufferIndex, int offset, object data)
        {
            Gl.NamedBufferSubData(
                attributeBufferIds[bufferIndex],
                new IntPtr(offset * GetBufferSize(data)), // TODO: not right if updating with an array..
                GetBufferSize(data),
                data);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Gl.DeleteBuffers(this.attributeBufferIds);
            Gl.DeleteVertexArrays(this.id);
            GC.SuppressFinalize(this);
        }

        private static uint GetBufferSize(object data)
        {
            switch (data)
            {
                case Vector3 v3:
                    return (uint)(sizeof(float) * 3);
                case Vector3[] v3a:
                    return (uint)(sizeof(float) * 3 * v3a.Length);
                case Vector2 v2:
                    return (uint)(sizeof(float) * 2);
                case Vector2[] v2a:
                    return (uint)(sizeof(float) * 2 * v2a.Length);
                case uint[] uia:
                    return (uint)(sizeof(uint) * uia.Length);
                case float f:
                    return (uint)(sizeof(float));
                case float[] fa:
                    return (uint)(sizeof(float) * fa.Length);
                default:
                    throw new ArgumentException("Unsupported type.");
            }
        }

        private static VertexAttribType GetVertexAttribType(Type type)
        {
            if (type == typeof(Vector3[]))
            {
                return VertexAttribType.Float;
            }
            else if (type == typeof(Vector2[]))
            {
                return VertexAttribType.Float;
            }
            else if (type == typeof(float[]))
            {
                return VertexAttribType.Float;
            }
            else if (type == typeof(uint[]))
            {
                return VertexAttribType.UnsignedInt;
            }
            else
            {
                throw new ArgumentException("Unsupported type.");
            }
        }

        private static int GetVertexAttribMultiple(Type type)
        {
            if (type == typeof(Vector3[]))
            {
                return 3;
            }
            else if (type == typeof(Vector2[]))
            {
                return 2;
            }
            else if (type == typeof(float[]))
            {
                return 1;
            }
            else if (type == typeof(uint[]))
            {
                return 1;
            }
            else
            {
                throw new ArgumentException("Unsupported type.");
            }
        }
    }
}
