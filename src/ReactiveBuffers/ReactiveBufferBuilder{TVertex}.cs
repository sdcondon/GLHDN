using GLHDN.Core;
using GLHDN.Core.VaoDecorators;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GLHDN.ReactiveBuffers
{
    public class ReactiveBufferBuilder<TVertex>
    {
        private readonly VertexArrayObjectBuilder builder;
        private readonly IObservable<IObservable<IList<TVertex>>> vertexSource;
        private readonly IList<int> indices;

        public ReactiveBufferBuilder(
            PrimitiveType primitiveType,
            int atomCapacity,
            IList<int> indices,
            IObservable<IObservable<IList<TVertex>>> vertexSource)
        {
            var verticesPerAtom = indices.Max() + 1; // Perhaps should throw if has unused indices..

            builder = new VertexArrayObjectBuilder(primitiveType)
                .WithAttributeBuffer<TVertex>(BufferUsage.DynamicDraw, atomCapacity * verticesPerAtom)
                .WithIndex(atomCapacity * indices.Count)
                .Synchronize();

            this.vertexSource = vertexSource;
            this.indices = indices;
        }

        public ReactiveBuffer<TVertex> Build()
        {
            var vao = builder.Build();
            return new ReactiveBuffer<TVertex>(vao, vertexSource, indices);
        }
    }
}
