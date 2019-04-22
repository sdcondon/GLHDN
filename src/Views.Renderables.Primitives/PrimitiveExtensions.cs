namespace GLHDN.Views.Renderables.Primitives
{
    using System.Collections.Generic;
    using System.Numerics;

    public static class PrimitiveExtensions
    {
        public static void AddCuboid(this ICollection<Primitive> list, Vector3 size, Matrix4x4 worldTransform, Vector4 color)
        {
            list.Add(Primitive.Cuboid(size, worldTransform, color));
        }

        public static void AddQuad(this ICollection<Primitive> list, Vector2 size, Matrix4x4 worldTransform, Vector4 color)
        {
            list.Add(Primitive.Quad(size, worldTransform, color));
        }

        public static void AddLine(this ICollection<Primitive> list, Vector3 from, Vector3 to, Vector4 color)
        {
            list.Add(Primitive.Line(from, to, color));
        }

        public static void AddLine(this ICollection<Primitive> list, Vector3 from, Vector3 to, Vector4 colorFrom, Vector4 colorTo)
        {
            list.Add(Primitive.Line(from, to, colorFrom, colorTo));
        }

        public static void AddLineCircle(this ICollection<Primitive> list, float radius, Matrix4x4 worldTransform, Vector4 color)
        {
            list.Add(Primitive.LineCircle(radius, worldTransform, color));
        }

        public static void AddLineEllipse(this ICollection<Primitive> list, float radiusX, float radiusY, Matrix4x4 worldTransform, Vector4 color)
        {
            list.Add(Primitive.LineEllipse(radiusX, radiusY, worldTransform, color));
        }

        public static void AddLineSquare(this ICollection<Primitive> list, float sideLength, Matrix4x4 worldTransform, Vector4 color)
        {
            list.Add(Primitive.LineSquare(sideLength, worldTransform, color));
        }

        public static void AddLinePolygon(this ICollection<Primitive> list, Vector2[] positions, Matrix4x4 worldTransform, Vector4 color)
        {
            list.Add(Primitive.LinePolygon(positions, worldTransform, color));
        }
    }
}
