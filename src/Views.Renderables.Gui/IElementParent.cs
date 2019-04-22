namespace GLHDN.Views.Renderables.Gui
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Interface for types that contain GUI elements. This includes parent elements and the root GUI object.
    /// </summary>
    public interface IElementParent
    {
        /// <summary>
        /// Gets the elements that this object contains.
        /// </summary>
        IObservable<IObservable<Element>> SubElements { get; }

        /// <summary>
        /// Gets the position of the center of the element, in screen space.
        /// </summary>
        Vector2 Center { get; }

        /// <summary>
        /// Gets the size of the element, in screen space (i.e. pixels).
        /// </summary>
        Vector2 Size { get; }
    }
}
