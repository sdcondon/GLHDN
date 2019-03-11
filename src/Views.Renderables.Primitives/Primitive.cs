namespace GLHDN.Views.Renderables.Primitives
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public sealed class Primitive
    {
        private List<PrimitiveVertex> vertices = new List<PrimitiveVertex>();

        private Primitive(bool isTrianglePrimitive)
        {
            IsTrianglePrimitive = isTrianglePrimitive;
        }

        public IReadOnlyList<PrimitiveVertex> Vertices => vertices;

        public bool IsTrianglePrimitive { get; private set; }

        // todo: sphere

        public static Primitive Cuboid(Vector3 size, Matrix4x4 worldTransform, Vector4 color)
        {
            var xy = new Vector2(size.X, size.Y);
            var xz = new Vector2(size.X, size.Z);
            var zy = new Vector2(size.Z, size.Y);
            var zOffset = Matrix4x4.CreateTranslation(new Vector3(0, 0, .5f));

            var primitive = new Primitive(true);
            primitive.AddQuad(xy, zOffset * worldTransform, color);
            primitive.AddQuad(xy, zOffset * Matrix4x4.CreateRotationX((float)Math.PI) * worldTransform, color);
            primitive.AddQuad(xz, zOffset * Matrix4x4.CreateRotationX((float)-Math.PI / 2) * worldTransform, color);
            primitive.AddQuad(xz, zOffset * Matrix4x4.CreateRotationX((float)Math.PI / 2) * worldTransform, color);
            primitive.AddQuad(zy, zOffset * Matrix4x4.CreateRotationY((float)-Math.PI / 2) * worldTransform, color);
            primitive.AddQuad(zy, zOffset * Matrix4x4.CreateRotationY((float)Math.PI / 2) * worldTransform, color);
            return primitive;
        }

        public static Primitive Quad(Vector2 size, Matrix4x4 worldTransform, Vector4 color)
        {
            var primitive = new Primitive(true);
            primitive.AddQuad(size, worldTransform, color);
            return primitive;
        }

        public static Primitive Line(Vector3 from, Vector3 to, Vector4 color)
        {
            return Line(from, to, color, color);
        }

        public static Primitive Line(Vector3 from, Vector3 to, Vector4 colorFrom, Vector4 colorTo)
        {
            var primitive = new Primitive(false);
            primitive.AddVertex(from, colorFrom, Vector3.Zero);
            primitive.AddVertex(to, colorTo, Vector3.Zero);
            return primitive;
        }

        public static Primitive LineCircle(float radius, Matrix4x4 worldTransform, Vector4 color)
        {
            return LineEllipse(radius, radius, worldTransform, color);
        }

        public static Primitive LineEllipse(float radiusX, float radiusY, Matrix4x4 worldTransform, Vector4 color)
        {
            var primitive = new Primitive(false);

            var segments = 16;

            Vector3 getPos(int i)
            {
                var rads = (i % segments) * 2 * Math.PI / segments;
                return new Vector3((float)Math.Sin(rads) * radiusX, (float)Math.Cos(rads) * radiusY, 0);
            }

            for (var i = 0; i < segments; i++)
            {
                primitive.AddVertex(Vector3.Transform(getPos(i), worldTransform), color, Vector3.Zero);
                primitive.AddVertex(Vector3.Transform(getPos(i + 1), worldTransform), color, Vector3.Zero);
            }

            return primitive;
        }

        public static Primitive LineSquare(float sideLength, Matrix4x4 worldTransform, Vector4 color)
        {
            var primitive = new Primitive(false);
            primitive.AddVertex(Vector3.Transform(new Vector3(-sideLength/2, -sideLength/2, 0), worldTransform), color, Vector3.Zero);
            primitive.AddVertex(Vector3.Transform(new Vector3(-sideLength/2, +sideLength/2, 0), worldTransform), color, Vector3.Zero);
            primitive.AddVertex(Vector3.Transform(new Vector3(+sideLength/2, -sideLength/2, 0), worldTransform), color, Vector3.Zero);
            primitive.AddVertex(Vector3.Transform(new Vector3(+sideLength/2, +sideLength/2, 0), worldTransform), color, Vector3.Zero);
            primitive.AddVertex(Vector3.Transform(new Vector3(-sideLength/2, -sideLength/2, 0), worldTransform), color, Vector3.Zero);
            primitive.AddVertex(Vector3.Transform(new Vector3(+sideLength/2, -sideLength/2, 0), worldTransform), color, Vector3.Zero);
            primitive.AddVertex(Vector3.Transform(new Vector3(-sideLength/2, +sideLength/2, 0), worldTransform), color, Vector3.Zero);
            primitive.AddVertex(Vector3.Transform(new Vector3(+sideLength/2, +sideLength/2, 0), worldTransform), color, Vector3.Zero);
            return primitive;
        }

        public static Primitive LinePolygon(Vector2[] positions, Matrix4x4 worldTransform, Vector4 color)
        {
            var primitive = new Primitive(false);

            for (int i = 0; i < positions.Length; i++)
            {
                primitive.AddVertex(Vector3.Transform(new Vector3(positions[i], 0), worldTransform), color, Vector3.Zero);
                primitive.AddVertex(Vector3.Transform(new Vector3(positions[(i + 1) % positions.Length], 0), worldTransform), color, Vector3.Zero);
            }

            return primitive;
        }

        private void AddQuad(Vector2 size, Matrix4x4 worldTransform, Vector4 color)
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

        private void AddVertex(Vector3 position, Vector4 color, Vector3 normal)
        {
            vertices.Add(new PrimitiveVertex(position, color, normal));
        }
    }
}
