namespace GLHDN.Views.Renderables.Primitives
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    /// <summary>
    /// Container for primitive vertex data.
    /// </summary>
    public sealed class Primitive
    {
        private readonly List<PrimitiveVertex> vertices = new List<PrimitiveVertex>();

        private Primitive(bool isTrianglePrimitive)
        {
            IsTrianglePrimitive = isTrianglePrimitive;
        }

        /// <summary>
        /// Gets the list of vertices that comprise the primitive.
        /// </summary>
        public IReadOnlyList<PrimitiveVertex> Vertices => vertices;

        /// <summary>
        /// Gets a value indicating whether the primitive is comprised of triangles (as opposed to lines).
        /// </summary>
        public bool IsTrianglePrimitive { get; }

        /// <summary>
        /// Creates a cuboid primitive.
        /// </summary>
        /// <param name="size">The dimensions of the cuboid.</param>
        /// <param name="worldTransform">The world transform of the cuboid.</param>
        /// <param name="color">The color of the cuboid.</param>
        /// <returns>The created primitive.</returns>
        public static Primitive Cuboid(Vector3 size, Matrix4x4 worldTransform, Color color)
        {
            var xy = new Vector2(size.X, size.Y);
            var xz = new Vector2(size.X, size.Z);
            var zy = new Vector2(size.Z, size.Y);
            var xOffset = Matrix4x4.CreateTranslation(0, 0, size.X / 2);
            var yOffset = Matrix4x4.CreateTranslation(0, 0, size.Y / 2);
            var zOffset = Matrix4x4.CreateTranslation(0, 0, size.Z / 2);

            var primitive = new Primitive(true);
            primitive.AddQuad(xy, zOffset * worldTransform, color);
            primitive.AddQuad(xy, zOffset * Matrix4x4.CreateRotationX((float)Math.PI) * worldTransform, color);
            primitive.AddQuad(xz, yOffset * Matrix4x4.CreateRotationX((float)-Math.PI / 2) * worldTransform, color);
            primitive.AddQuad(xz, yOffset * Matrix4x4.CreateRotationX((float)Math.PI / 2) * worldTransform, color);
            primitive.AddQuad(zy, xOffset * Matrix4x4.CreateRotationY((float)-Math.PI / 2) * worldTransform, color);
            primitive.AddQuad(zy, xOffset * Matrix4x4.CreateRotationY((float)Math.PI / 2) * worldTransform, color);
            return primitive;
        }

        /// <summary>
        /// Creates a quad primitive.
        /// </summary>
        /// <param name="size">The dimensions of the quad.</param>
        /// <param name="worldTransform">The world transform of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        /// <returns>The created primitive.</returns>
        public static Primitive Quad(Vector2 size, Matrix4x4 worldTransform, Color color)
        {
            var primitive = new Primitive(true);
            primitive.AddQuad(size, worldTransform, color);
            return primitive;
        }

        /// <summary>
        /// Creates a line primitive of constant color.
        /// </summary>
        /// <param name="from">The position of one end of the line.</param>
        /// <param name="to">The position of the other end of the line.</param>
        /// <param name="color">The color of the line.</param>
        /// <returns>The created primitive.</returns>
        public static Primitive Line(Vector3 from, Vector3 to, Color color)
        {
            return Line(from, to, color, color);
        }

        /// <summary>
        /// Creates a line primitive of graduated color.
        /// </summary>
        /// <param name="from">The position of one end of the line.</param>
        /// <param name="to">The position of the other end of the line.</param>
        /// <param name="colorFrom">The color of one end of the line.</param>
        /// <param name="colorTo">The color of the other end of the line.</param>
        /// <returns>The created primitive.</returns>
        public static Primitive Line(Vector3 from, Vector3 to, Color colorFrom, Color colorTo)
        {
            var primitive = new Primitive(false);
            primitive.AddVertex(from, colorFrom, Vector3.Zero);
            primitive.AddVertex(to, colorTo, Vector3.Zero);
            return primitive;
        }

        /// <summary>
        /// Creates a line circle primitive.
        /// </summary>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="worldTransform">The world transform of the circle.</param>
        /// <param name="color">The color of the circle.</param>
        /// <returns>The created primitive.</returns>
        public static Primitive LineCircle(float radius, Matrix4x4 worldTransform, Color color)
        {
            return LineEllipse(radius, radius, worldTransform, color);
        }

        /// <summary>
        /// Creates a line ellipse primitive.
        /// </summary>
        /// <param name="radiusX">The X-axis radius of the ellipse.</param>
        /// <param name="radiusY">The Y-axis radius of the ellipse.</param>
        /// <param name="worldTransform">The world transform of the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <returns>The created primitive.</returns>
        public static Primitive LineEllipse(float radiusX, float radiusY, Matrix4x4 worldTransform, Color color)
        {
            var primitive = new Primitive(false);

            var segments = 16;

            Vector3 GetPos(int i)
            {
                var rads = (i % segments) * 2 * Math.PI / segments;
                return new Vector3((float)Math.Sin(rads) * radiusX, (float)Math.Cos(rads) * radiusY, 0);
            }

            for (var i = 0; i < segments; i++)
            {
                primitive.AddVertex(Vector3.Transform(GetPos(i), worldTransform), color, Vector3.Zero);
                primitive.AddVertex(Vector3.Transform(GetPos(i + 1), worldTransform), color, Vector3.Zero);
            }

            return primitive;
        }

        /// <summary>
        /// Creates a line square primitive.
        /// </summary>
        /// <param name="sideLength">The side length the square.</param>
        /// <param name="worldTransform">The world transform of the square.</param>
        /// <param name="color">The color of the square.</param>
        /// <returns>The created primitive.</returns>
        public static Primitive LineSquare(float sideLength, Matrix4x4 worldTransform, Color color)
        {
            var primitive = new Primitive(false);
            primitive.AddVertex(Vector3.Transform(new Vector3(-sideLength / 2, -sideLength / 2, 0), worldTransform), color, Vector3.Zero);
            primitive.AddVertex(Vector3.Transform(new Vector3(-sideLength / 2, +sideLength / 2, 0), worldTransform), color, Vector3.Zero);
            primitive.AddVertex(Vector3.Transform(new Vector3(+sideLength / 2, -sideLength / 2, 0), worldTransform), color, Vector3.Zero);
            primitive.AddVertex(Vector3.Transform(new Vector3(+sideLength / 2, +sideLength / 2, 0), worldTransform), color, Vector3.Zero);
            primitive.AddVertex(Vector3.Transform(new Vector3(-sideLength / 2, -sideLength / 2, 0), worldTransform), color, Vector3.Zero);
            primitive.AddVertex(Vector3.Transform(new Vector3(+sideLength / 2, -sideLength / 2, 0), worldTransform), color, Vector3.Zero);
            primitive.AddVertex(Vector3.Transform(new Vector3(-sideLength / 2, +sideLength / 2, 0), worldTransform), color, Vector3.Zero);
            primitive.AddVertex(Vector3.Transform(new Vector3(+sideLength / 2, +sideLength / 2, 0), worldTransform), color, Vector3.Zero);
            return primitive;
        }

        /// <summary>
        /// Creates a line polygon primitive.
        /// </summary>
        /// <param name="positions">The positions of the vertices of the polygon.</param>
        /// <param name="worldTransform">The world transform of the polygon.</param>
        /// <param name="color">The color of the polygon.</param>
        /// <returns>The created primitive.</returns>
        public static Primitive LinePolygon(Vector2[] positions, Matrix4x4 worldTransform, Color color)
        {
            var primitive = new Primitive(false);

            for (int i = 0; i < positions.Length; i++)
            {
                primitive.AddVertex(Vector3.Transform(new Vector3(positions[i], 0), worldTransform), color, Vector3.Zero);
                primitive.AddVertex(Vector3.Transform(new Vector3(positions[(i + 1) % positions.Length], 0), worldTransform), color, Vector3.Zero);
            }

            return primitive;
        }

        private void AddQuad(Vector2 size, Matrix4x4 worldTransform, Color color)
        {
            var normal = Vector3.TransformNormal(Vector3.UnitZ, worldTransform);
            var vertexPositions = new[]
            {
                new Vector3(-size.X / 2, -size.Y / 2, 0),
                new Vector3(+size.X / 2, -size.Y / 2, 0),
                new Vector3(-size.X / 2, +size.Y / 2, 0),

                new Vector3(+size.X / 2, +size.Y / 2, 0),
                new Vector3(-size.X / 2, +size.Y / 2, 0),
                new Vector3(+size.X / 2, -size.Y / 2, 0),
            };

            for (int i = 0; i < vertexPositions.Length; i++)
            {
                AddVertex(Vector3.Transform(vertexPositions[i], worldTransform), color, normal);
            }
        }

        private void AddVertex(Vector3 position, Color color, Vector3 normal)
        {
            vertices.Add(new PrimitiveVertex(position, color, normal));
        }
    }
}
