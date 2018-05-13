namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents an Open GL vertex array object.
    /// </summary>
    public sealed class GlVertexArrayObject : IDisposable
    {
        private readonly uint id;
        private readonly PrimitiveType primitiveType;

        private readonly VertexAttribBuffer[] attributeBuffers;
        private readonly uint? indexBufferId;
        private readonly int vertexCount;

        internal GlVertexArrayObject(
            PrimitiveType primitiveType,
            IList<BufferUsage> attributeUsages,
            IList<Array> attributeData,
            uint[] indexData)
        {
            //  Record primitive type for use in draw calls, create and bind the VAO
            this.primitiveType = primitiveType;
            this.id = Gl.GenVertexArray();
            Gl.BindVertexArray(id);

            // Create and populate the attribute buffers
            this.attributeBuffers = new VertexAttribBuffer[attributeUsages.Count]; // TODO: Assert length consistency?
            for (int i = 0; i < attributeBuffers.Length; i++)
            {
                this.attributeBuffers[i] = new VertexAttribBuffer(attributeData[i], attributeUsages[i]);
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
                this.vertexCount = attributeData[0].Length;
            }
        }

        /// <summary>
        /// Finalizer. Releases any unmanaged resources used by an object as it is GC'd.
        /// </summary>
        ~GlVertexArrayObject()
        {
            Gl.DeleteVertexArrays(this.id);
        }

        public IReadOnlyList<VertexAttribBuffer> Buffers => this.attributeBuffers;

        /// <summary>
        /// Draw with the active program. TODO: Allow specification of buffer binding?
        /// </summary>
        public void Draw(int count = -1)
        {
            Gl.BindVertexArray(this.id);

            // Set the attribute pointers..
            for (uint i = 0, k = 0; i < attributeBuffers.Length; i++)
            {
                Gl.BindBuffer(BufferTarget.ArrayBuffer, attributeBuffers[i].Id);
                for (uint j = 0; j < attributeBuffers[i].Attributes.Length; j++, k++)
                {
                    Gl.EnableVertexAttribArray(k);
                    Gl.VertexAttribPointer(
                        index: k, // must match the layout in the shader
                        size: attributeBuffers[i].Attributes[j].multiple,
                        type: attributeBuffers[i].Attributes[j].type,
                        normalized: false,
                        stride: attributeBuffers[i].Attributes[j].stride,
                        pointer: attributeBuffers[i].Attributes[j].offset);
                }
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
            for (uint i = 0, k = 0; i < attributeBuffers.Length; i++)
            {
                for (uint j = 0; j < attributeBuffers[i].Attributes.Length; j++, k++)
                {
                    Gl.DisableVertexAttribArray(k);
                }
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

        public sealed class VertexAttribBuffer
        {
            internal VertexAttribBuffer(Array data, BufferUsage usage)
            {
                var elementType = data.GetType().GetElementType();
                this.Attributes = VertexAttribInfo.ForType(elementType);

                this.Id = Gl.GenBuffer();
                Gl.BindBuffer(BufferTarget.ArrayBuffer, this.Id);
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Marshal.SizeOf(elementType) * data.Length), data, usage);
            }

            ~VertexAttribBuffer()
            {
                Gl.DeleteBuffers(this.Id);
            }

            public void SetSubData(int offset, object data)
            {
                Gl.NamedBufferSubData(
                    buffer: Id,
                    offset: new IntPtr(offset * Marshal.SizeOf(data)),
                    size: (uint)Marshal.SizeOf(data),
                    data: data);
            }

            internal uint Id { get; private set; }

            internal VertexAttribInfo[] Attributes { get; private set; }

            internal void Dispose()
            {
                Gl.DeleteBuffers(this.Id);
                GC.SuppressFinalize(this);
            }
        }

        internal struct VertexAttribInfo
        {
            public IntPtr offset;
            public int stride;
            public VertexAttribType type;
            public int multiple;

            private VertexAttribInfo(VertexAttribType type, int multiple, int offset, int stride)
            {
                this.offset = new IntPtr(offset);
                this.stride = stride;
                this.type = type;
                this.multiple = multiple;
            }

            public static VertexAttribInfo[] ForType(Type t)
            {
                var attributes = new List<VertexAttribInfo>();
                ForType(t, attributes, 0, Marshal.SizeOf(t));
                return attributes.ToArray();
            }

            private static void ForType(Type t, List<VertexAttribInfo> attributes, int offset, int stride)
            {
                if (t == typeof(Vector4))
                {
                    attributes.Add(new VertexAttribInfo(VertexAttribType.Float, 4, offset, stride));
                }
                else if (t == typeof(Vector3))
                {
                    attributes.Add(new VertexAttribInfo(VertexAttribType.Float, 3, offset, stride));
                }
                else if (t == typeof(Vector2))
                {
                    attributes.Add(new VertexAttribInfo(VertexAttribType.Float, 2, offset, stride));
                }
                else if (t == typeof(float))
                {
                    attributes.Add(new VertexAttribInfo(VertexAttribType.Float, 1, offset, stride));
                }
                else if (t == typeof(uint))
                {
                    attributes.Add(new VertexAttribInfo(VertexAttribType.UnsignedInt, 1, offset, stride));
                }
                else if (!t.IsValueType || t.IsAutoLayout)
                {
                    throw new ArgumentException("Unsupported type - passed type must be blittable");
                }
                else
                {
                    foreach (var field in t.GetFields())
                    {
                        ForType(field.FieldType, attributes, offset + (int)Marshal.OffsetOf(t, field.Name), stride);
                    }
                }
            }
        }
    }
}
