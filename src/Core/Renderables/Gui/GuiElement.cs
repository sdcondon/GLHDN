namespace OpenGlHelpers.Core
{
    using System.Numerics;

    public class GuiElement
    {
        public Vector3 Color { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Vector2 TopLeft { get; set; }
        public Vector2 TopRight => new Vector2(TopLeft.X + Width, TopLeft.Y);
        public Vector2 BottomLeft => new Vector2(TopLeft.X, TopLeft.Y + Height);
        public Vector2 BottomRight => new Vector2(TopLeft.X + Width, TopLeft.Y + Height);
    }
}
