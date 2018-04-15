namespace GLHDN.Views
{
    using System.Numerics;

    public static class IViewContextExtensions
    {
        public static Vector2 GetCenter(this IViewContext context)
        {
            return new Vector2(context.Width / 2, context.Height / 2);
        }
    }
}
