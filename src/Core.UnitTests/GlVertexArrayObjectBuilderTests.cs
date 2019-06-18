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
                .Replace(() => new GlVertexArrayObject(Is.A<PrimitiveType>(), Is.A<IList<(BufferUsage, Type, int, Array)>>(), Is.A<(int, uint[])>()))
                .With((PrimitiveType t, IList<(BufferUsage, Type, int, Array)> b, (int, uint[]) i) => new MemoryVertexArrayObject(t, b, i));

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
