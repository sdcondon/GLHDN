using GLHDN.Core;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GLHDN.ReactiveBuffers
{
    /// <summary>
    /// Builder for <see cref="ReactiveBuffer{TVertex}"/> instances.
    /// </summary>
    /// <typeparam name="TVertex">The vertex type of the buffer to be built.</typeparam>
    public class ReactiveBufferBuilder<TVertex>
    {
        private readonly VertexArrayObjectBuilder builder;
        private readonly IObservable<IObservable<IList<TVertex>>> vertexSource;
        private readonly IList<int> indices;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactiveBufferBuilder{TVertex}"/> class.
        /// </summary>
        /// <param name="primitiveType"></param>
        /// <param name="atomCapacity"></param>
        /// <param name="indices"></param>
        /// <param name="atomSource"></param>
        public ReactiveBufferBuilder(
            PrimitiveType primitiveType,
            int atomCapacity,
            IList<int> indices,
            IObservable<IObservable<IList<TVertex>>> atomSource)
        {
            var verticesPerAtom = indices.Max() + 1; // Perhaps should throw if has unused indices..

            builder = new VertexArrayObjectBuilder(primitiveType)
                .WithAttributeBuffer<TVertex>(BufferUsage.DynamicDraw, atomCapacity * verticesPerAtom)
                .WithIndex(atomCapacity * indices.Count)
                .Synchronized();

            this.vertexSource = atomSource;
            this.indices = indices;
        }

        /// <summary>
        /// Builds a <see cref="ReactiveBuffer{TVertex}"/> instance based on the state of this builder.
        /// </summary>
        /// <returns>The new <see cref="ReactiveBuffer{TVertex}"/> instance.</returns>
        public ReactiveBuffer<TVertex> Build()
        {
            var vao = builder.Build();
            return new ReactiveBuffer<TVertex>(vao, vertexSource, indices);
        }
    }
}
