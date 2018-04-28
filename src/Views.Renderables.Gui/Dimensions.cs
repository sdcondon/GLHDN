namespace GLHDN.Views.Renderables.Gui
{
    using System.Numerics;

    public struct Dimensions
    {
        private Vector2 value;

        public Dimensions(int absoluteX, int absoluteY)
        {
            value = new Vector2(absoluteX, absoluteY);
            IsXRelative = false;
            IsYRelative = false;
        }

        public Dimensions(int absoluteX, float relativeY)
        {
            value = new Vector2(absoluteX, relativeY);
            IsXRelative = false;
            IsYRelative = true;
        }

        public Dimensions(float relativeX, int absoluteY)
        {
            value = new Vector2(relativeX, absoluteY);
            IsXRelative = true;
            IsYRelative = false;
        }

        public Dimensions(float relativeX, float relativeY)
        {
            value = new Vector2(relativeX, relativeY);
            IsXRelative = true;
            IsYRelative = true;
        }

        public float X => value.X;

        public float Y => value.Y;

        public bool IsXRelative { get; }

        public bool IsYRelative { get; }
    }
}
