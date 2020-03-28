using System.Numerics;

namespace GLHDN.Views.Renderables.Gui
{
    /// <summary>
    /// Container for information about a GUI element vertex
    /// </summary>
    public struct Vertex
    {
        public readonly Vector2 position;
        public readonly Vector4 color;
        public readonly float texZ;
        public readonly Vector2 texXY;

        public Vertex(Vector2 position, Vector4 color)
        {
            this.position = position;
            this.color = color;
            this.texZ = -1;
            this.texXY = Vector2.Zero;
        }

        public Vertex(Vector2 position, Vector4 color, int texZ, Vector2 texXY)
        {
            this.position = position;
            this.color = color;
            this.texZ = texZ;
            this.texXY = texXY;
        }
    }
}
