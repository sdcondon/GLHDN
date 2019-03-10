namespace GLHDN.Views.Renderables.Primitives._3d
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public sealed class Primitive
    {
        private List<PrimitiveVertex> vertices = new List<PrimitiveVertex>();

        public IReadOnlyList<PrimitiveVertex> Vertices => vertices;

        // todo: sphere

        //public static Primitive Cuboid(Vector3 size, Matrix4x4 worldTransform, Vector4 color)
        //{

        //}

        public static Primitive Quad(Vector2 size, Matrix4x4 worldTransform, Vector4 color)
        {
            var primitive = new Primitive();
            primitive.AddQuad(size, worldTransform, color);
            return primitive;
        }

        //public static Primitive Line(Vector2 from, Vector2 to, Vector4 color)
        //{
        //    return Line(from, to, color, color);
        //}

        //public static Primitive Line(Vector2 from, Vector2 to, Vector4 colorFrom, Vector4 colorTo)
        //{
        //    var primitive = new Primitive();
        //    primitive.AddVertex(from, colorFrom);
        //    primitive.AddVertex(to, colorTo);
        //    return primitive;
        //}

        private void AddQuad(Vector2 size, Matrix4x4 worldTransform, Vector4 color)
        {
            var vertexPositions = new[]
            {
                new Vector3(-size.X / 2, -size.Y / 2, 0),
                new Vector3(+size.X / 2, -size.Y / 2, 0),
                new Vector3(-size.X / 2, +size.Y / 2, 0),

                new Vector3(+size.X / 2, +size.Y / 2, 0),
                new Vector3(-size.X / 2, +size.Y / 2, 0),
                new Vector3(+size.X / 2, -size.Y / 2, 0),
            };

            var normal = Vector3.TransformNormal(Vector3.UnitZ, worldTransform);

            for (int i = 0; i < vertexPositions.Length; i++)
            {
                AddVertex(
                    Vector3.Transform(vertexPositions[i], worldTransform),
                    color,
                    Vector3.TransformNormal(vertexPositions[i], worldTransform));
            }
        }

        private void AddVertex(Vector3 position, Vector4 color, Vector3 normal)
        {
            vertices.Add(new PrimitiveVertex(position, color, normal));
        }
    }
}
