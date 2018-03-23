namespace OpenGlHelpers.Core
{
    using OpenGL;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Numerics;

    public sealed class VertexArrayObjectBuilder
    {
        private List<BufferTarget> bufferTargets = new List<BufferTarget>();
        private List<BufferUsage> bufferUsages = new List<BufferUsage>();
        private List<ICollection> bufferData = new List<ICollection>();
        private List<Type> bufferTypes = new List<Type>();
        private uint[] indexData;

        public VertexArrayObjectBuilder WithBuffer(BufferTarget target, BufferUsage usage, ICollection data)
        {
            bufferTargets.Add(target);
            bufferUsages.Add(usage);
            bufferData.Add(data);
            bufferTypes.Add(data.GetType());
            return this;
        }

        public VertexArrayObjectBuilder WithIndex(uint[] data)
        {
            indexData = data;
            return this;
        }

        public VertexArrayObject Create(PrimitiveType primitiveType)
        {
            uint id = Gl.GenVertexArray();
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

            return new VertexArrayObject(
                id,
                primitiveType,
                bufferIds,
                bufferTypes.ToArray(),
                elementCount,
                indexBufferId);
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
    }
}
