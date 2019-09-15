namespace GLHDN.Core
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
                object[] MakeTestCase(Type type, params GlVertexAttributeInfo[] expectedAttributeInfo)
                    => new object[] { type, expectedAttributeInfo };

                return new[]
                {
                    MakeTestCase(
                        type: typeof(Vector4),
                        expectedAttributeInfo: new GlVertexAttributeInfo(VertexAttribType.Float, 4, 0, 16)),
                    MakeTestCase(
                        type: typeof(Vector3),
                        expectedAttributeInfo: new GlVertexAttributeInfo(VertexAttribType.Float, 3, 0, 12)),
                    MakeTestCase(
                        type: typeof(Vector2),
                        expectedAttributeInfo: new GlVertexAttributeInfo(VertexAttribType.Float, 2, 0, 8)),
                    MakeTestCase(
                        type: typeof(float),
                        expectedAttributeInfo: new GlVertexAttributeInfo(VertexAttribType.Float, 1, 0, 4)),
                    MakeTestCase(
                        type: typeof(uint),
                        expectedAttributeInfo: new GlVertexAttributeInfo(VertexAttribType.UnsignedInt, 1, 0, 4)),
                };
            }
        }

        [Theory]
        [MemberData(nameof(ForType_ValidInput_TestCases))]
        public void ForType_WithValidInput_ReturnsCorrectOutput(Type type, GlVertexAttributeInfo[] expectedAttributeInfo)
        {
            GlVertexAttributeInfo.ForType(type).Should().BeEquivalentTo(expectedAttributeInfo);
        }
    }
}
