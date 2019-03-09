namespace GLHDN.Views.Renderables.Primitives._2d
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public sealed class Primitive
    {
        private List<PrimitiveVertex> vertices = new List<PrimitiveVertex>();

        public IReadOnlyList<PrimitiveVertex> Vertices => vertices;

        public static Primitive Circle(Vector2 centre, float radius, Vector4 color)
        {
            return Ellipse(centre, radius, radius, color);
        }

        public static Primitive Ellipse(Vector2 centre, float radiusX, float radiusY, Vector4 color)
        {
            var primitive = new Primitive();

            var segments = 16;

            Vector2 getPos(int i)
            {
                var rads = (i % segments) * 2 * Math.PI / segments;
                return centre + new Vector2((float)Math.Sin(rads) * radiusX, (float)Math.Cos(rads) * radiusY);
            }

            for (var i = 0; i < segments; i++)
            {
                primitive.AddVertex(getPos(i), color);
                primitive.AddVertex(getPos(i + 1), color);
            }

            return primitive;
        }

        public static Primitive Square(Vector2 centre, float sideLength, Vector4 color)
        {
            var primitive = new Primitive();

            primitive.AddVertex(centre.X - sideLength / 2, centre.Y - sideLength / 2, color);
            primitive.AddVertex(centre.X - sideLength / 2, centre.Y + sideLength / 2, color);

            primitive.AddVertex(centre.X + sideLength / 2, centre.Y - sideLength / 2, color);
            primitive.AddVertex(centre.X + sideLength / 2, centre.Y + sideLength / 2, color);

            primitive.AddVertex(centre.X - sideLength / 2, centre.Y - sideLength / 2, color);
            primitive.AddVertex(centre.X + sideLength / 2, centre.Y - sideLength / 2, color);

            primitive.AddVertex(centre.X - sideLength / 2, centre.Y + sideLength / 2, color);
            primitive.AddVertex(centre.X + sideLength / 2, centre.Y + sideLength / 2, color);

            return primitive;
        }

        public static Primitive Polygon(Vector2[] positions, Vector4 color)
        {
            var primitive = new Primitive();

            for (int i = 0; i < positions.Length; i++)
            {
                primitive.AddVertex(positions[i], color);
                primitive.AddVertex(positions[(i + 1) % positions.Length], color);
            }

            return primitive;
        }

        public static Primitive Line(Vector2 from, Vector2 to, Vector4 color)
        {
            return Line(from, to, color, color);
        }

        public static Primitive Line(Vector2 from, Vector2 to, Vector4 colorFrom, Vector4 colorTo)
        {
            var primitive = new Primitive();
            primitive.AddVertex(from, colorFrom);
            primitive.AddVertex(to, colorTo);
            return primitive;
        }

        private void AddVertex(Vector2 position, Vector4 color)
        {
            vertices.Add(new PrimitiveVertex(new Vector3(position.X, position.Y, 0), color));
        }

        private void AddVertex(float x, float y, Vector4 color)
        {
            vertices.Add(new PrimitiveVertex(new Vector3(x, y, 0), color));
        }
    }
}
