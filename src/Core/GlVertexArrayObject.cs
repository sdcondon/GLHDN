namespace GLHDN.Core
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

        private readonly Buffer[] attributeBuffers;
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
            this.attributeBuffers = new Buffer[attributeUsages.Count]; // TODO: Assert length consistency?
            for (int i = 0; i < attributeBuffers.Length; i++)
            {
                this.attributeBuffers[i] = new Buffer(attributeData[i], attributeUsages[i]);
            }

            // Establish element count & populate index buffer if there is one
            if (indexData != null)
            {
                this.indexBufferId = Gl.GenBuffer();
                Gl.BindBuffer(BufferTarget.ElementArrayBuffer, this.indexBufferId.Value);
                Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(sizeof(uint) * indexData.Length), indexData, BufferUsage.DynamicDraw);
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
            Gl.DeleteVertexArrays(this.id);
        }

        public IReadOnlyList<Buffer> Buffers => this.attributeBuffers;

        /// <summary>
        /// Draw with the active program. TODO: Allow specification of buffer binding?
        /// </summary>
        public void Draw(int count = -1)
        {
            Gl.BindVertexArray(this.id);

            // Set the attribute pointers..
            for (uint i = 0; i < attributeBuffers.Length; i++)
            {
                Gl.EnableVertexAttribArray(i);
                Gl.BindBuffer(BufferTarget.ArrayBuffer, attributeBuffers[i].Id);
                Gl.VertexAttribPointer(
                    index: i, // must match the layout in the shader
                    size: attributeBuffers[i].Info.multiple,
                    type: attributeBuffers[i].Info.type,
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
            for (uint i = 0; i < attributeBuffers.Length; i++)
            {
                Gl.DisableVertexAttribArray(i);
            }
        }

        public void SetIndexData(int offset, uint data)
        {
            Gl.NamedBufferSubData(
                this.indexBufferId.Value,
                new IntPtr(offset * sizeof(uint)), // TODO: not right if updating with an array..
                sizeof(uint),
                data);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach(var buffer in attributeBuffers)
            {
                buffer.Dispose();
            }

            Gl.DeleteVertexArrays(this.id);
            GC.SuppressFinalize(this);
        }

        public sealed class Buffer
        {
            internal Buffer(ICollection data, BufferUsage usage)
            {
                this.Id = Gl.GenBuffer();
                Gl.BindBuffer(BufferTarget.ArrayBuffer, Id);
                Gl.BufferData(BufferTarget.ArrayBuffer, GetBufferSize(data), data, usage);
                this.Info = VertexAttribInfo.ForType(data.GetType());
            }

            ~Buffer()
            {
                Gl.DeleteBuffers(this.Id);
            }

            public void SetSubData(int offset, object data)
            {
                Gl.NamedBufferSubData(
                    Id,
                    new IntPtr(offset * GetBufferSize(data)), // TODO: not right if updating with an array..
                    GetBufferSize(data),
                    data);
            }

            internal uint Id { get; private set; }

            internal VertexAttribInfo Info { get; private set; }

            internal void Dispose()
            {
                Gl.DeleteBuffers(this.Id);
                GC.SuppressFinalize(this);
            }

            private static uint GetBufferSize(object data)
            {
                switch (data)
                {
                    case Vector4 v4:
                        return (uint)(sizeof(float) * 4);
                    case Vector4[] v4a:
                        return (uint)(sizeof(float) * 4 * v4a.Length);
                    case Vector3 v3:
                        return (uint)(sizeof(float) * 3);
                    case Vector3[] v3a:
                        return (uint)(sizeof(float) * 3 * v3a.Length);
                    case Vector2 v2:
                        return (uint)(sizeof(float) * 2);
                    case Vector2[] v2a:
                        return (uint)(sizeof(float) * 2 * v2a.Length);
                    case uint ui:
                        return (uint)(sizeof(uint));
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

            internal struct VertexAttribInfo
            {
                public VertexAttribType type;
                public int multiple;

                public VertexAttribInfo(VertexAttribType type, int multiple)
                {
                    this.type = type;
                    this.multiple = multiple;
                }

                public static VertexAttribInfo ForType(Type type)
                {
                    if (type == typeof(Vector4[]))
                    {
                        return new VertexAttribInfo(VertexAttribType.Float, 4);
                    }
                    else if (type == typeof(Vector3[]))
                    {
                        return new VertexAttribInfo(VertexAttribType.Float, 3);
                    }
                    else if (type == typeof(Vector2[]))
                    {
                        return new VertexAttribInfo(VertexAttribType.Float, 2);
                    }
                    else if (type == typeof(float[]))
                    {
                        return new VertexAttribInfo(VertexAttribType.Float, 1);
                    }
                    else if (type == typeof(uint[]))
                    {
                        return new VertexAttribInfo(VertexAttribType.UnsignedInt, 1);
                    }
                    else
                    {
                        throw new ArgumentException("Unsupported type.");
                    }
                }
            }
        }
    }
}
