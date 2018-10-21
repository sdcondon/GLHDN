namespace GLHDN.Views.Renderables.Gui
{
    using System.Collections.Generic;
    using System.Numerics;

    public interface IElementParent
    {
        ICollection<Element> SubElements { get; }

        /// <summary>
        /// Gets the position of the center of the element, in screen space.
        /// </summary>
        Vector2 Center_ScreenSpace { get; }

        /// <summary>
        /// Gets the size of the element, in screen space (i.e. pixels).
        /// </summary>
        Vector2 Size_ScreenSpace { get; }
    }
}
