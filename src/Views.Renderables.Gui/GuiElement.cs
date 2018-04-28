namespace GLHDN.Views.Renderables.Gui
{
    using System;
    using System.Numerics;

    /// <summary>
    /// 
    /// </summary>
    public class GuiElement : IGuiElement
    {
        public GuiElement(IGuiElement parent)
        {
            this.Parent = parent;
        }

        public IGuiElement Parent { get; internal set; }

        /// <summary>
        /// Gets or sets the position in parent-space of the local origin.
        /// </summary>
        public Dimensions ParentOrigin { get; set; }

        /// <summary>
        /// Gets or sets the position relative to the control that will be placed at the parent origin.
        /// </summary>
        public Dimensions LocalOrigin { get; set; }

        /// <summary>
        /// Gets or sets the size of the element.
        /// </summary>
        public Dimensions Size { get; set; }

        /// <summary>
        /// Gets or sets the background color of the element.
        /// </summary>
        public Vector4 Color { get; set; }

        /// <summary>
        /// Gets or sets the width of the border of the element.
        /// </summary>
        public float BorderWidth { get; set; }

        /// <inheritdoc />
        public Vector2 Center
        {
            get
            {
                var parentOriginScreenSpace = new Vector2(
                    Parent.Center.X + (ParentOrigin.IsXRelative ? ParentOrigin.X * Parent.ScreenSize.X / 2 : ParentOrigin.X),
                    Parent.Center.Y + (ParentOrigin.IsYRelative ? ParentOrigin.Y * Parent.ScreenSize.Y / 2 : ParentOrigin.Y));

                return new Vector2(
                    parentOriginScreenSpace.X - (LocalOrigin.IsXRelative ? LocalOrigin.X * ScreenSize.X / 2 : LocalOrigin.X),
                    parentOriginScreenSpace.Y - (LocalOrigin.IsYRelative ? LocalOrigin.Y * ScreenSize.Y / 2 : LocalOrigin.Y));
            }
        }

        /// <inheritdoc />
        public Vector2 ScreenSize
        {
            get
            {
                return new Vector2(
                    Size.IsXRelative ? Parent.ScreenSize.X * Size.X : Size.X,
                    Size.IsYRelative ? Parent.ScreenSize.Y * Size.Y : Size.Y);
            }
        }

        /// <summary>
        /// Action to be invoked whenever an element property is updated. Used to e.g. update OpenGL buffers when the element changes.
        /// </summary>
        internal Action<GuiElement> Updated { get; set; }

        // TODO: handlers - onclick, onmouseover, onmouseout etc
    }
}
