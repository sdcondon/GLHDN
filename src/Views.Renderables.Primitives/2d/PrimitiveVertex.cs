namespace GLHDN.Views.Renderables.Primitives._2d
{
    using System.Numerics;

    public struct PrimitiveVertex
    {
        public readonly Vector3 Position;
        public readonly Vector4 Color;

        public PrimitiveVertex(Vector3 position, Vector4 color)
        {
            this.Position = position;
            this.Color = color;
        }
    }
}
