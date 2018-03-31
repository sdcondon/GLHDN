namespace OpenGlHelpers.Core
{
    using OpenGL;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class VertexArrayObjectBuilder
    {
        private readonly PrimitiveType primitiveType;
        private readonly List<BufferTarget> bufferTargets = new List<BufferTarget>();
        private readonly List<BufferUsage> bufferUsages = new List<BufferUsage>();
        private readonly List<ICollection> bufferData = new List<ICollection>();
        private uint[] indexData;

        public VertexArrayObjectBuilder(PrimitiveType primitiveType)
        {
            this.primitiveType = primitiveType;
        }

        public VertexArrayObjectBuilder WithBuffer(BufferTarget target, BufferUsage usage, ICollection data)
        {
            bufferTargets.Add(target);
            bufferUsages.Add(usage);
            bufferData.Add(data);
            return this;
        }

        public VertexArrayObjectBuilder WithIndex(uint[] data)
        {
            indexData = data;
            return this;
        }

        public VertexArrayObject Build()
        {
            return new VertexArrayObject(
                primitiveType,
                bufferTargets,
                bufferUsages,
                bufferData,
                indexData);
        }
    }
}
