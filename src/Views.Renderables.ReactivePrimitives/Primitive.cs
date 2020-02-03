using System;
using System.Collections.Generic;
using System.Numerics;

namespace GLHDN.Views.Renderables.ReactivePrimitives
{
    /// <summary>
    /// Container for primitive vertex data.
    /// </summary>
    /// <remarks>
    /// TODO: Perhaps should be an immutable struct instead to discourage heap allocations..
    /// </remarks>
    public sealed class Primitive
    {
        private readonly List<PrimitiveVertex> vertices = new List<PrimitiveVertex>();

        private Primitive(Action<Primitive> initialize)
        {
            initialize?.Invoke(this);
        }

        /// <summary>
        /// Gets the list of vertices that comprise the primitive.
        /// </summary>
        public IReadOnlyList<PrimitiveVertex> Vertices => vertices;

        /// <summary>
        /// Gets a value indicating whether the primitive is comprised of triangles (as opposed to lines).
        /// </summary>
        public bool IsTrianglePrimitive { get; private set; }

        /// <summary>
        /// Creates an empty primitive.
        /// </summary>
        /// <returns>The created primitive.</returns>
        public static Primitive Empty() => new Primitive(null);

        /// <summary>
        /// Creates a cuboid primitive.
        /// </summary>
        /// <param name="size">The dimensions of the cuboid.</param>
        /// <param name="worldTransform">The world transform of the cuboid.</param>
        /// <param name="color">The color of the cuboid.</param>
        /// <returns>The created primitive.</returns>
        public static Primitive Cuboid(Vector3 size, Matrix4x4 worldTransform, Color color) => new Primitive(p => p.SetCuboid(size, worldTransform, color));

        /// <summary>
        /// Creates a quad primitive.
        /// </summary>
        /// <param name="size">The dimensions of the quad.</param>
        /// <param name="worldTransform">The world transform of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        /// <returns>The created primitive.</returns>
        public static Primitive Quad(Vector2 size, Matrix4x4 worldTransform, Color color) => new Primitive(p => p.SetQuad(size, worldTransform, color));

        /// <summary>
        /// Creates a line primitive of constant color.
        /// </summary>
        /// <param name="from">The position of one end of the line.</param>
        /// <param name="to">The position of the other end of the line.</param>
        /// <param name="color">The color of the line.</param>
        /// <returns>The created primitive.</returns>
        public static Primitive Line(Vector3 from, Vector3 to, Color color) => new Primitive(p => p.SetLine(from, to, color));

        /// <summary>
        /// Creates a line primitive of graduated color.
        /// </summary>
        /// <param name="from">The position of one end of the line.</param>
        /// <param name="to">The position of the other end of the line.</param>
        /// <param name="colorFrom">The color of one end of the line.</param>
        /// <param name="colorTo">The color of the other end of the line.</param>
        /// <returns>The created primitive.</returns>
        public static Primitive Line(Vector3 from, Vector3 to, Color colorFrom, Color colorTo) => new Primitive(p => p.SetLine(from, to, colorFrom, colorTo));

        /// <summary>
        /// Creates a line circle primitive.
        /// </summary>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="worldTransform">The world transform of the circle.</param>
        /// <param name="color">The color of the circle.</param>
        /// <returns>The created primitive.</returns>
        public static Primitive LineCircle(float radius, Matrix4x4 worldTransform, Color color) => LineEllipse(radius, radius, worldTransform, color);

        /// <summary>
        /// Creates a line ellipse primitive.
        /// </summary>
        /// <param name="radiusX">The X-axis radius of the ellipse.</param>
        /// <param name="radiusY">The Y-axis radius of the ellipse.</param>
        /// <param name="worldTransform">The world transform of the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <returns>The created primitive.</returns>
        public static Primitive LineEllipse(float radiusX, float radiusY, Matrix4x4 worldTransform, Color color) => new Primitive(p => p.SetLineEllipse(radiusX, radiusY, worldTransform, color));

        /// <summary>
        /// Creates a line square primitive.
        /// </summary>
        /// <param name="sideLength">The side length the square.</param>
        /// <param name="worldTransform">The world transform of the square.</param>
        /// <param name="color">The color of the square.</param>
        /// <returns>The created primitive.</returns>
        public static Primitive LineSquare(float sideLength, Matrix4x4 worldTransform, Color color) => new Primitive(p => p.SetLineSquare(sideLength, worldTransform, color));

        /// <summary>
        /// Creates a line polygon primitive.
        /// </summary>
        /// <param name="positions">The positions of the vertices of the polygon.</param>
        /// <param name="worldTransform">The world transform of the polygon.</param>
        /// <param name="color">The color of the polygon.</param>
        /// <returns>The created primitive.</returns>
        public static Primitive LinePolygon(Vector2[] positions, Matrix4x4 worldTransform, Color color) => new Primitive(p => p.SetLinePolygon(positions, worldTransform, color));

        /// <summary>
        /// Sets primitive as a cuboid.
        /// </summary>
        /// <param name="size">The dimensions of the cuboid.</param>
        /// <param name="worldTransform">The world transform of the cuboid.</param>
        /// <param name="color">The color of the cuboid.</param>
        public void SetCuboid(Vector3 size, Matrix4x4 worldTransform, Color color)
        {
            IsTrianglePrimitive = true;
            vertices.Clear();

            var xy = new Vector2(size.X, size.Y);
            var xz = new Vector2(size.X, size.Z);
            var zy = new Vector2(size.Z, size.Y);
            var xOffset = Matrix4x4.CreateTranslation(0, 0, size.X / 2);
            var yOffset = Matrix4x4.CreateTranslation(0, 0, size.Y / 2);
            var zOffset = Matrix4x4.CreateTranslation(0, 0, size.Z / 2);

            AddQuad(xy, zOffset * worldTransform, color);
            AddQuad(xy, zOffset * Matrix4x4.CreateRotationX((float)Math.PI) * worldTransform, color);
            AddQuad(xz, yOffset * Matrix4x4.CreateRotationX((float)-Math.PI / 2) * worldTransform, color);
            AddQuad(xz, yOffset * Matrix4x4.CreateRotationX((float)Math.PI / 2) * worldTransform, color);
            AddQuad(zy, xOffset * Matrix4x4.CreateRotationY((float)-Math.PI / 2) * worldTransform, color);
            AddQuad(zy, xOffset * Matrix4x4.CreateRotationY((float)Math.PI / 2) * worldTransform, color);
        }

        /// <summary>
        /// Sets primitive as a quad.
        /// </summary>
        /// <param name="size">The dimensions of the quad.</param>
        /// <param name="worldTransform">The world transform of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        public void SetQuad(Vector2 size, Matrix4x4 worldTransform, Color color)
        {
            IsTrianglePrimitive = true;
            vertices.Clear();

            AddQuad(size, worldTransform, color);
        }

        /// <summary>
        /// Sets primitive as a line of constant color.
        /// </summary>
        /// <param name="from">The position of one end of the line.</param>
        /// <param name="to">The position of the other end of the line.</param>
        /// <param name="color">The color of the line.</param>
        public void SetLine(Vector3 from, Vector3 to, Color color)
        {
            SetLine(from, to, color, color);
        }

        /// <summary>
        /// Sets primitive as a line of graduated color.
        /// </summary>
        /// <param name="from">The position of one end of the line.</param>
        /// <param name="to">The position of the other end of the line.</param>
        /// <param name="colorFrom">The color of one end of the line.</param>
        /// <param name="colorTo">The color of the other end of the line.</param>
        public void SetLine(Vector3 from, Vector3 to, Color colorFrom, Color colorTo)
        {
            IsTrianglePrimitive = false;
            vertices.Clear();

            AddVertex(from, colorFrom, Vector3.Zero);
            AddVertex(to, colorTo, Vector3.Zero);
        }

        /// <summary>
        /// Sets primitive as a line circle.
        /// </summary>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="worldTransform">The world transform of the circle.</param>
        /// <param name="color">The color of the circle.</param>
        public void SetLineCircle(float radius, Matrix4x4 worldTransform, Color color)
        {
            SetLineEllipse(radius, radius, worldTransform, color);
        }

        /// <summary>
        /// Sets primitive as a line ellipse.
        /// </summary>
        /// <param name="radiusX">The X-axis radius of the ellipse.</param>
        /// <param name="radiusY">The Y-axis radius of the ellipse.</param>
        /// <param name="worldTransform">The world transform of the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        public void SetLineEllipse(float radiusX, float radiusY, Matrix4x4 worldTransform, Color color)
        {
            IsTrianglePrimitive = false;
            vertices.Clear();

            var segments = 16;

            Vector3 GetPos(int i)
            {
                var rads = (i % segments) * 2 * Math.PI / segments;
                return new Vector3((float)Math.Sin(rads) * radiusX, (float)Math.Cos(rads) * radiusY, 0);
            }

            for (var i = 0; i < segments; i++)
            {
                AddVertex(Vector3.Transform(GetPos(i), worldTransform), color, Vector3.Zero);
                AddVertex(Vector3.Transform(GetPos(i + 1), worldTransform), color, Vector3.Zero);
            }
        }

        /// <summary>
        /// Sets primitive as line square.
        /// </summary>
        /// <param name="sideLength">The side length the square.</param>
        /// <param name="worldTransform">The world transform of the square.</param>
        /// <param name="color">The color of the square.</param>
        public void SetLineSquare(float sideLength, Matrix4x4 worldTransform, Color color)
        {
            IsTrianglePrimitive = false;
            vertices.Clear();

            AddVertex(Vector3.Transform(new Vector3(-sideLength / 2, -sideLength / 2, 0), worldTransform), color, Vector3.Zero);
            AddVertex(Vector3.Transform(new Vector3(-sideLength / 2, +sideLength / 2, 0), worldTransform), color, Vector3.Zero);
            AddVertex(Vector3.Transform(new Vector3(+sideLength / 2, -sideLength / 2, 0), worldTransform), color, Vector3.Zero);
            AddVertex(Vector3.Transform(new Vector3(+sideLength / 2, +sideLength / 2, 0), worldTransform), color, Vector3.Zero);
            AddVertex(Vector3.Transform(new Vector3(-sideLength / 2, -sideLength / 2, 0), worldTransform), color, Vector3.Zero);
            AddVertex(Vector3.Transform(new Vector3(+sideLength / 2, -sideLength / 2, 0), worldTransform), color, Vector3.Zero);
            AddVertex(Vector3.Transform(new Vector3(-sideLength / 2, +sideLength / 2, 0), worldTransform), color, Vector3.Zero);
            AddVertex(Vector3.Transform(new Vector3(+sideLength / 2, +sideLength / 2, 0), worldTransform), color, Vector3.Zero);
        }

        /// <summary>
        /// Sets primitive as a line polygon.
        /// </summary>
        /// <param name="positions">The positions of the vertices of the polygon.</param>
        /// <param name="worldTransform">The world transform of the polygon.</param>
        /// <param name="color">The color of the polygon.</param>
        public void SetLinePolygon(Vector2[] positions, Matrix4x4 worldTransform, Color color)
        {
            IsTrianglePrimitive = false;
            vertices.Clear();

            for (int i = 0; i < positions.Length; i++)
            {
                AddVertex(Vector3.Transform(new Vector3(positions[i], 0), worldTransform), color, Vector3.Zero);
                AddVertex(Vector3.Transform(new Vector3(positions[(i + 1) % positions.Length], 0), worldTransform), color, Vector3.Zero);
            }
        }

        private void AddQuad(Vector2 size, Matrix4x4 worldTransform, Color color)
        {
            var normal = Vector3.TransformNormal(Vector3.UnitZ, worldTransform);

            AddVertex(Vector3.Transform(new Vector3(-size.X / 2, -size.Y / 2, 0), worldTransform), color, normal);
            AddVertex(Vector3.Transform(new Vector3(+size.X / 2, -size.Y / 2, 0), worldTransform), color, normal);
            AddVertex(Vector3.Transform(new Vector3(-size.X / 2, +size.Y / 2, 0), worldTransform), color, normal);
            AddVertex(Vector3.Transform(new Vector3(+size.X / 2, +size.Y / 2, 0), worldTransform), color, normal);
            AddVertex(Vector3.Transform(new Vector3(-size.X / 2, +size.Y / 2, 0), worldTransform), color, normal);
            AddVertex(Vector3.Transform(new Vector3(+size.X / 2, -size.Y / 2, 0), worldTransform), color, normal);
        }

        private void AddVertex(Vector3 position, Color color, Vector3 normal)
        {
            vertices.Add(new PrimitiveVertex(position, color, normal));
        }
    }
}
