using System.Collections.Generic;
using System.Numerics;

namespace GLHDN.Views.Renderables.ReactivePrimitives
{
    /// <summary>
    /// Extension methods for the <see cref="Primitive"/> class.
    /// </summary>
    public static class PrimitiveCollectionExtensions
    {
        /// <summary>
        /// Adds a cuboid to a collection of primitives.
        /// </summary>
        /// <param name="list">The list to add the cuboid to.</param>
        /// <param name="size">The dimensions of the cuboid.</param>
        /// <param name="worldTransform">The world transform of the cuboid.</param>
        /// <param name="color">The color of the cuboid.</param>
        public static void AddCuboid(this ICollection<Primitive> list, Vector3 size, Matrix4x4 worldTransform, Color color)
        {
            list.Add(Primitive.Cuboid(size, worldTransform, color));
        }

        /// <summary>
        /// Adds a quad to a collection of primitives.
        /// </summary>
        /// <param name="list">The list to add the quad to.</param>
        /// <param name="size">The dimensions of the quad.</param>
        /// <param name="worldTransform">The world transform of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        public static void AddQuad(this ICollection<Primitive> list, Vector2 size, Matrix4x4 worldTransform, Color color)
        {
            list.Add(Primitive.Quad(size, worldTransform, color));
        }

        /// <summary>
        /// Adds a line segment to a collection of primitives.
        /// </summary>
        /// <param name="list">The list to add the line segment to.</param>
        /// <param name="from">The position of one end of the line.</param>
        /// <param name="to">The position of the other end of the line.</param>
        /// <param name="color">The color of the line.</param>
        public static void AddLine(this ICollection<Primitive> list, Vector3 from, Vector3 to, Color color)
        {
            list.Add(Primitive.Line(from, to, color));
        }

        /// <summary>
        /// Adds a line segment to a collection of primitives.
        /// </summary>
        /// <param name="list">The list to add the line segment to.</param>
        /// <param name="from">The position of one end of the line.</param>
        /// <param name="to">The position of the other end of the line.</param>
        /// <param name="colorFrom">The color of one end of the line.</param>
        /// <param name="colorTo">The color of the other end of the line.</param>
        public static void AddLine(this ICollection<Primitive> list, Vector3 from, Vector3 to, Color colorFrom, Color colorTo)
        {
            list.Add(Primitive.Line(from, to, colorFrom, colorTo));
        }

        /// <summary>
        /// Adds a line circle to a collection of primitives.
        /// </summary>
        /// <param name="list">The list to add the line circle to.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="worldTransform">The world transform of the circle.</param>
        /// <param name="color">The color of the circle.</param>
        public static void AddLineCircle(this ICollection<Primitive> list, float radius, Matrix4x4 worldTransform, Color color)
        {
            list.Add(Primitive.LineCircle(radius, worldTransform, color));
        }

        /// <summary>
        /// Adds a line ellipse to a collection of primitives.
        /// </summary>
        /// <param name="list">The list to add the line ellipse to.</param>
        /// <param name="radiusX">The X-axis radius of the ellipse.</param>
        /// <param name="radiusY">The Y-axis radius of the ellipse.</param>
        /// <param name="worldTransform">The world transform of the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        public static void AddLineEllipse(this ICollection<Primitive> list, float radiusX, float radiusY, Matrix4x4 worldTransform, Color color)
        {
            list.Add(Primitive.LineEllipse(radiusX, radiusY, worldTransform, color));
        }

        /// <summary>
        /// Adds a line square to a collection of primitives.
        /// </summary>
        /// <param name="list">The list to add the line square to.</param>
        /// <param name="sideLength">The side length the square.</param>
        /// <param name="worldTransform">The world transform of the square.</param>
        /// <param name="color">The color of the square.</param>
        public static void AddLineSquare(this ICollection<Primitive> list, float sideLength, Matrix4x4 worldTransform, Color color)
        {
            list.Add(Primitive.LineSquare(sideLength, worldTransform, color));
        }

        /// <summary>
        /// Adds a line polygon to a collection of primitives.
        /// </summary>
        /// <param name="list">The list to add the line polygon to.</param>
        /// <param name="positions">The positions of the vertices of the polygon.</param>
        /// <param name="worldTransform">The world transform of the polygon.</param>
        /// <param name="color">The color of the polygon.</param>
        public static void AddLinePolygon(this ICollection<Primitive> list, Vector2[] positions, Matrix4x4 worldTransform, Color color)
        {
            list.Add(Primitive.LinePolygon(positions, worldTransform, color));
        }
    }
}
