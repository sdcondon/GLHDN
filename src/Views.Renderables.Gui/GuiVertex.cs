using System.Numerics;

namespace GLHDN.Views.Renderables.Gui
{
    /// <summary>
    /// Container for information about a GUI element vertex
    /// </summary>
    public struct GuiVertex
    {
        public readonly Vector2 position;
        public readonly Vector4 color;
        public readonly Vector2 elementPosition;
        public readonly Vector2 elementSize;
        public readonly float borderWidth; // could be done with two quads instead? different levels of abstraction..

        public GuiVertex(Vector2 position, Vector4 color, Vector2 elementOrigin, Vector2 elementSize, float borderWidth)
        {
            this.position = position;
            this.color = color;
            this.elementPosition = position - elementOrigin;
            this.elementSize = elementSize;
            this.borderWidth = borderWidth;
        }

        public GuiVertex(Vector2 position, Vector4 color, Vector2 elementOrigin, Vector2 elementSize, int texZ)
        {
            this.position = position;
            this.color = color;
            this.elementPosition = position - elementOrigin;
            this.elementSize = elementSize;
            this.borderWidth = -texZ;
        }
    }
}
