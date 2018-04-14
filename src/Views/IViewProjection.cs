namespace GLHDN.Views
{
    using System.Numerics;

    /// <summary>
    /// A source of view and projection matrices. Used by some <see cref="IRenderable"/> implementations.
    /// </summary>
    public interface IViewProjection
    {
        Matrix4x4 View { get; }

        Matrix4x4 Projection { get; }
    }
}