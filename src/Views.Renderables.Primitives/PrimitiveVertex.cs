namespace GLHDN.Views.Renderables.Primitives
{
    using System.Numerics;

    public struct PrimitiveVertex
    {
        public readonly Vector3 Position;
        public readonly Vector4 Color;
        public readonly Vector3 Normal;

        public PrimitiveVertex(Vector3 position, Vector4 color, Vector3 normal)
        {
            this.Position = position;
            this.Color = color;
            this.Normal = normal;
        }
    }
}
