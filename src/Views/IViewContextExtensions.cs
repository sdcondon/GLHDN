namespace GLHDN.Views
{
    using System.Numerics;

    /// <summary>
    /// Extension methods for <see cref="IViewContext"/> instances.
    /// </summary>
    public static class IViewContextExtensions
    {
        /// <summary>
        /// Gets the position of the center of a context, given its size.
        /// </summary>
        /// <param name="context">The context to get the center position of.</param>
        /// <returns>The position of the center of the context, given its size.</returns>
        public static Vector2 GetCenter(this IViewContext context)
        {
            return new Vector2(context.Width / 2, context.Height / 2);
        }
    }
}
