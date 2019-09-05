using GLHDN.Core;
using OpenGL;
using System.Collections.Generic;
using System.Linq;

namespace GLHDN.ReactiveBuffers
{
    public static class GlVertexArrayObjectBuilderExtensions
    {
        public static GlVertexArrayObjectBuilder ForReactiveBuffer<TVertex>(
            this GlVertexArrayObjectBuilder builder,
            int atomCapacity,
            IList<int> indices)
        {
            var verticesPerAtom = indices.Max() + 1; // Perhaps should throw if has unused indices..

            return builder
                .WithAttributeBuffer<TVertex>(BufferUsage.DynamicDraw, atomCapacity * verticesPerAtom)
                .WithIndex(atomCapacity * indices.Count);
        }
    }
}
