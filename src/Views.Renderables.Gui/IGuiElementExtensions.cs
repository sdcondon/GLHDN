namespace GLHDN.Views.Renderables.Gui
{
    using System.Numerics;

    public static class IGuiElementExtensions
    {
        public static Vector2 GetPosBL(this IGuiElement e)
        {
            return e.Center_ScreenSpace - e.Size_ScreenSpace / 2;
        }

        public static Vector2 GetPosBR(this IGuiElement e)
        {
            return new Vector2(e.Center_ScreenSpace.X + e.Size_ScreenSpace.X / 2, e.Center_ScreenSpace.Y - e.Size_ScreenSpace.Y / 2);
        }

        public static Vector2 GetPosTL(this IGuiElement e)
        {
            return new Vector2(e.Center_ScreenSpace.X - e.Size_ScreenSpace.X / 2, e.Center_ScreenSpace.Y + e.Size_ScreenSpace.Y / 2);
        }

        public static Vector2 GetPosTR(this IGuiElement e)
        {
            return e.Center_ScreenSpace + e.Size_ScreenSpace / 2;
        }
    }
}
