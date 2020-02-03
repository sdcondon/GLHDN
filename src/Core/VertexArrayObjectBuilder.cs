using GLHDN.Core.VaoDecorators;
using OpenGL;
using System;
using System.Collections.Generic;

namespace GLHDN.Core
{
    /// <summary>
    /// Builder class for <see cref="GlVertexArrayObject"/> objects that presents a fluent-ish interface.
    /// </summary>
    /// <remarks>
    /// Useful for setting up a VAO before the OpenGL context has initialized.
    /// </remarks>
    public sealed class VertexArrayObjectBuilder
    {
        private readonly PrimitiveType primitiveType;
        private readonly List<(BufferUsage usage, Type elementType, int elementCount, Array data)> bufferSpecs = new List<(BufferUsage, Type, int, Array)>();
        private (int count, uint[] data) indexSpec;
        private Func<PrimitiveType, List<(BufferUsage, Type, int, Array)>, (int count, uint[] data), IVertexArrayObject> build;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexArrayObjectBuilder"/> class.
        /// </summary>
        /// <param name="primitiveType">The type of primitive data to be stored in the built VAO.</param>
        public VertexArrayObjectBuilder(PrimitiveType primitiveType)
        {
            this.primitiveType = primitiveType;
            this.build = (primType, bufferSpecs, indexSpec) => new GlVertexArrayObject(primType, bufferSpecs, indexSpec);
        }

        /// <summary>
        /// Adds a new populated attribute buffer to be included in the built VAO.
        /// </summary>
        /// <param name="bufferUsage">The usage type for the buffer.</param>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder WithAttributeBuffer(BufferUsage bufferUsage, Array data)
        {
            this.bufferSpecs.Add((bufferUsage, data.GetType().GetElementType(), data.Length, data));
            return this;
        }

        /// <summary>
        /// Adds a new empty attribute buffer to be included in the built VAO.
        /// </summary>
        /// <typeparam name="T">The type of data to be stored in the buffer.</typeparam>
        /// <param name="bufferUsage">The usage type for the buffer.</param>
        /// <param name="size">The size of the buffer, in bytes.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder WithAttributeBuffer<T>(BufferUsage bufferUsage, int size)
        {
            this.bufferSpecs.Add((bufferUsage, typeof(T), size, null));
            return this;
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="data">The data with which to populate the buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder WithIndex(uint[] data)
        {
            this.indexSpec = (data.Length, data);
            return this;
        }

        /// <summary>
        /// Sets the index buffer to be included in the built VAO.
        /// </summary>
        /// <param name="capacity">The size of the index buffer.</param>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder WithIndex(int capacity)
        {
            this.indexSpec = (capacity, null);
            return this;
        }

        /// <summary>
        /// Specifies that the built VAO should be synchronized, with any pending changes flushed on each draw call. TODO: Hacky and needlessly bad for performance - look into streaming..
        /// </summary>
        /// <returns>The updated builder.</returns>
        public VertexArrayObjectBuilder Synchronized()
        {
            var innerBuild = build;
            build = (primType, bufferSpecs, indexSpec) => new SynchronizedVao(innerBuild(primType, bufferSpecs, indexSpec));
            return this;
        }

        /// <summary>
        /// Builds a new <see cref="GlVertexArrayObject"/> instance based on the state of the builder.
        /// </summary>
        /// <returns>The built VAO.</returns>
        public IVertexArrayObject Build()
        {
            return build(primitiveType, bufferSpecs, indexSpec);
        }
    }
}
