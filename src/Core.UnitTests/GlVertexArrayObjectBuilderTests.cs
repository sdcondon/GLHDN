namespace GLHDN.Core.UnitTests
{
    using OpenGL;
    using Pose;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class GlVertexArrayObjectBuilderTests
    {
        [Fact]
        public void Test()
        {
            Shim ctorShim = Shim
                .Replace(() => new GlVertexArrayObject(Is.A<PrimitiveType>(), Is.A<IList<Tuple<BufferUsage, Array>>>(), Is.A<uint[]>()))
                .With((PrimitiveType t, IList<Tuple<BufferUsage, Array>> b, uint[] i) => new MockVertexArrayObject(t, b, i));

            PoseContext.Isolate(() =>
            {
                var subject = new GlVertexArrayObjectBuilder(PrimitiveType.Triangles);
                subject.WithAttributeBuffer(BufferUsage.DynamicDraw, new[] { 0 });
                subject.WithIndex(new[] { 0u });
                var built = subject.Build();
            }, ctorShim);
        }
    }
}
