namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject"/> objects that presents a fluent-ish interface.
    /// </summary>
    public sealed class GlVertexArrayObjectBuilder
    {
        private readonly PrimitiveType primitiveType;
        private readonly List<Func<GlVertexBufferObject>> bufferBuilders = new List<Func<GlVertexBufferObject>>();
        private uint[] indexData;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlVertexArrayObjectBuilder"/> class.
        /// </summary>
        /// <param name="primitiveType"></param>
        public GlVertexArrayObjectBuilder(PrimitiveType primitiveType)
        {
            this.primitiveType = primitiveType;
        }

        public GlVertexArrayObjectBuilder WithAttributeBuffer(BufferUsage bufferUsage, Array data)
        {
            this.bufferBuilders.Add(() => new GlVertexBufferObject(bufferUsage, data));
            return this;
        }

        public GlVertexArrayObjectBuilder WithIndex(uint[] data)
        {
            this.indexData = data;
            return this;
        }

        public GlVertexArrayObject Build()
        {
            return new GlVertexArrayObject(primitiveType, bufferBuilders, indexData);
        }
    }
}
