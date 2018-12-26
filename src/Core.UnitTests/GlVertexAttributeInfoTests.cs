namespace GLHDN.Core.UnitTests
{
    using FluentAssertions;
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using Xunit;

    public class GlVertexAttributeInfoTests
    {
        public static IEnumerable<object[]> ForType_ValidInput_TestCases
        {
            get
            {
                object[] makeTestCase(Type type, params GlVertexAttributeInfo[] expectedAttributeinfo)
                    => new object[] { type, expectedAttributeinfo };

                return new[]
                {
                    makeTestCase(
                        typeof(Vector4),
                        new GlVertexAttributeInfo(VertexAttribType.Float, 4, 0, 16)),
                    makeTestCase(
                        typeof(Vector3),
                        new GlVertexAttributeInfo(VertexAttribType.Float, 3, 0, 12)),
                    makeTestCase(
                        typeof(Vector2),
                        new GlVertexAttributeInfo(VertexAttribType.Float, 2, 0, 8)),
                    makeTestCase(
                        typeof(float),
                        new GlVertexAttributeInfo(VertexAttribType.Float, 1, 0, 4)),
                    makeTestCase(
                        typeof(uint),
                        new GlVertexAttributeInfo(VertexAttribType.UnsignedInt, 1, 0, 4)),
                };
            }
        }

        [Theory]
        [MemberData(nameof(ForType_ValidInput_TestCases))]
        public void ForType_WithValidInput_ReturnsCorrectOutput(Type type, GlVertexAttributeInfo[] expectedAttributeinfo)
        {
            GlVertexAttributeInfo.ForType(type).Should().BeEquivalentTo(expectedAttributeinfo);
        }
    }
}
