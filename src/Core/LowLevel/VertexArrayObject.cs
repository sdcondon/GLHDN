namespace OpenGlHelpers.Core
{
    using OpenGL;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public sealed class VertexArrayObject : IDisposable
    {
        private readonly uint id;
        private readonly PrimitiveType primitiveType;

        private readonly uint[] bufferIds;
        private readonly int[] bufferElementSizes;
        private readonly VertexAttribType[] bufferElementTypes;
        private readonly int elementCount;
        private readonly uint? indexBufferId;

        internal VertexArrayObject(
            PrimitiveType primitiveType,
            List<BufferTarget> bufferTargets,
            List<BufferUsage> bufferUsages,
            List<ICollection> bufferData,
            uint[] indexData)
        {
            this.id = Gl.GenVertexArray();
            Gl.BindVertexArray(id);

            var bufferIds = new uint[bufferTargets.Count];
            Gl.GenBuffers(bufferIds);
            for (int i = 0; i < bufferTargets.Count; i++)
            {
                Gl.BindBuffer(bufferTargets[i], bufferIds[i]);
                Gl.BufferData(bufferTargets[i], GetSize(bufferData[i]), bufferData[i], bufferUsages[i]);
            }

            int elementCount;
            uint? indexBufferId = null;
            if (indexData != null)
            {
                indexBufferId = Gl.GenBuffer();
                Gl.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId.Value);
                Gl.BufferData(BufferTarget.ElementArrayBuffer, GetSize(indexData), indexData, BufferUsage.StaticDraw);
                elementCount = indexData.Length;
            }
            else
            {
                elementCount = bufferData[0].Count;
            }

            this.primitiveType = primitiveType;
            this.bufferIds = bufferIds;
            this.bufferElementSizes = bufferData.Select(a => GetElementSize(a.GetType())).ToArray();
            this.bufferElementTypes = bufferData.Select(a => GetElementType(a.GetType())).ToArray();
            this.elementCount = elementCount;
            this.indexBufferId = indexBufferId;
        }

        ~VertexArrayObject()
        {
            Gl.DeleteBuffers(this.bufferIds); // TODO: fix me - can't reliably reference ref type in finalizer.
            Gl.DeleteVertexArrays(this.id);
        }

        /// <summary>
        /// Draw with the active program.
        /// </summary>
        public void Draw()
        {
            Gl.BindVertexArray(this.id);

            for (uint i = 0; i < bufferIds.Length; i++)
            {
                Gl.EnableVertexAttribArray(i);
                Gl.BindBuffer(BufferTarget.ArrayBuffer, bufferIds[i]);
                Gl.VertexAttribPointer(
                    i,                     // attribute. Must match the layout in the shader.
                    bufferElementSizes[i], // size
                    bufferElementTypes[i], // type
                    false,             // normalized?
                    0,                 // stride
                    IntPtr.Zero);      // array buffer offset
            }

            // TODO: delegate instead of if every time?
            if (indexBufferId.HasValue)
            {
                // Bind index buffer and draw
                Gl.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId.Value);
                Gl.DrawElements(
                    this.primitiveType,           // mode
                    this.elementCount,            // count
                    DrawElementsType.UnsignedInt, // type
                    IntPtr.Zero);                 // element array buffer offset
            }
            else
            {
                Gl.DrawArrays(
                    this.primitiveType, // mode
                    0,                  // first
                    this.elementCount); // count
            }

            for (uint i = 0; i < bufferIds.Length; i++)
            {
                Gl.DisableVertexAttribArray(i);
            }
        }

        public void BufferSubData(int bufferIndex, uint offset, float data)
        {
            Gl.NamedBufferSubData(bufferIds[bufferIndex], new IntPtr(offset * sizeof(float)), (uint)sizeof(float), data);
        }

        public void BufferSubData(int bufferIndex, uint offset, Vector3 data)
        {
            Gl.NamedBufferSubData(bufferIds[bufferIndex], new IntPtr(offset * 3 * sizeof(float)), 3 * (uint)sizeof(float), data);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Gl.DeleteBuffers(this.bufferIds);
            Gl.DeleteVertexArrays(this.id);
            GC.SuppressFinalize(this);
        }

        private uint GetSize(object data)
        {
            switch (data)
            {
                case Vector3[] v3:
                    return (uint)(sizeof(float) * 3 * v3.Length);
                case Vector2[] v2:
                    return (uint)(sizeof(float) * 2 * v2.Length);
                case uint[] ui:
                    return (uint)(sizeof(uint) * ui.Length);
                case float[] f:
                    return (uint)(sizeof(float) * f.Length);
                default:
                    throw new ArgumentException("Unsupported type.");
            }
        }

        private int GetElementSize(Type type)
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

        private VertexAttribType GetElementType(Type type)
        {
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
        }
    }
}
