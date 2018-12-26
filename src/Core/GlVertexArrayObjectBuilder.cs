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
        /// <param name="primitiveType">The type of primitive data to be stored in the built VAO.</param>
        public GlVertexArrayObjectBuilder(PrimitiveType primitiveType)
        {
            this.primitiveType = primitiveType;
        }

        /// <summary>
        /// Adds an attribute buffer to be included in the built VAO.
        /// </summary>
        /// <param name="bufferUsage">The usage type for the buffer.</param>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public GlVertexArrayObjectBuilder WithAttributeBuffer(BufferUsage bufferUsage, Array data)
        {
            this.bufferSpecs.Add(Tuple.Create(bufferUsage, data));
            return this;
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public GlVertexArrayObjectBuilder WithIndex(uint[] data)
        {
            this.indexData = data;
            return this;
        }

        /// <summary>
        /// Builds a new <see cref="GlVertexArrayObject"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built VAO.</returns>
        public GlVertexArrayObject Build()
        {
            return new GlVertexArrayObject(primitiveType, bufferSpecs, indexData);
        }
    }
}
