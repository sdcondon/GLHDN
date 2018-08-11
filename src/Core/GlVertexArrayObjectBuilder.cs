namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <remarks>
    /// Useful for setting up a VAO before the OpenGL context has initialized.
    /// </remarks>
    public sealed class GlVertexArrayObjectBuilder
    {
        private readonly PrimitiveType primitiveType;
        private readonly List<Tuple<BufferUsage, Array>> bufferSpecs = new List<Tuple<BufferUsage, Array>>();
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
            this.bufferSpecs.Add(Tuple.Create(bufferUsage, data));
            return this;
        }

        public GlVertexArrayObjectBuilder WithIndex(uint[] data)
        {
            this.indexData = data;
            return this;
        }

        public GlVertexArrayObject Build()
        {
            return new GlVertexArrayObject(primitiveType, bufferSpecs, indexData);
        }
    }
}
