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
        // or public readonly Vector4 elementData { get; } // size/position or tex uv
        public readonly float borderWidth; // could be done with two quads instead? different levels of abstraction..

        public GuiVertex(Vector2 position, Vector4 color, Vector2 elementPosition, Vector2 elementSize, float borderWidth)
        {
            this.position = position;
            this.color = color;
            this.elementPosition = elementPosition;
            this.elementSize = elementSize;
            this.borderWidth = borderWidth;
        }
    }
}
