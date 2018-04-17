namespace GLHDN.Views.Renderables.Gui
{
    using System.Numerics;

    public static class IGuiElementExtensions
    {
        public static Vector2 BottomLeft(this IGuiElement e)
        {
            return e.Center - e.ScreenSize / 2;
        }

        public static Vector2 BottomRight(this IGuiElement e)
        {
            return new Vector2(e.Center.X + e.ScreenSize.X / 2, e.Center.Y - e.ScreenSize.Y / 2);
        }

        public static Vector2 TopLeft(this IGuiElement e)
        {
            return new Vector2(e.Center.X - e.ScreenSize.X / 2, e.Center.Y + e.ScreenSize.Y / 2);
        }

        public static Vector2 TopRight(this IGuiElement e)
        {
            return e.Center + e.ScreenSize / 2;
        }
    }
}
