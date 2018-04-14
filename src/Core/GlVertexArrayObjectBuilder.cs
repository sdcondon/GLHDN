namespace GLHDN.Core
{
    using OpenGL;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject"/> objects that presents a fluent-ish interface.
    /// </summary>
    public sealed class GlVertexArrayObjectBuilder
    {
        private readonly PrimitiveType primitiveType;

        private readonly List<BufferUsage> attributeUsages = new List<BufferUsage>();
        private readonly List<ICollection> attributeData = new List<ICollection>();
        private uint[] indexData;

        public GlVertexArrayObjectBuilder(PrimitiveType primitiveType)
        {
            this.primitiveType = primitiveType;
        }

        public GlVertexArrayObjectBuilder WithAttribute(BufferUsage usage, ICollection data)
        {
            attributeUsages.Add(usage);
            attributeData.Add(data);
            return this;
        }

        public GlVertexArrayObjectBuilder WithIndex(uint[] data)
        {
            indexData = data;
            return this;
        }

        public GlVertexArrayObject Build()
        {
            return new GlVertexArrayObject(
                primitiveType,
                attributeUsages,
                attributeData,
                indexData);
        }
    }
}
