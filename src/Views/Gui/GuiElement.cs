namespace GLHDN.Views
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
        public Vector2 ParentOrigin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parent origin's X-coordinate is relative [-1, 1] or absolute [-parentSize.X / 2, parentSize.X / 2].
        /// </summary>
        public bool IsParentOriginXRelative { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parent origin's Y-coordinate is relative [-1, 1] or absolute [-parentSize.Y / 2, parentSize.Y / 2].
        /// </summary>
        public bool IsParentOriginYRelative { get; set; }

        /// <summary>
        /// Gets or sets the position relative to the control that will be placed at the parent origin.
        /// </summary>
        public Vector2 LocalOrigin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the local origin's X-coordinate is relative [-1, 1] or absolute [-elementSize.X / 2, elementSize.X / 2].
        /// </summary>
        public bool IsLocalOriginXRelative { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the local origin's Y-coordinate is relative [-1, 1] or absolute [-elementSize.Y / 2, elementSize.Y / 2].
        /// </summary>
        public bool IsLocalOriginYRelative { get; set; }

        /// <summary>
        /// Gets or sets the size of the element.
        /// </summary>
        public Vector2 Size { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the size's X-coordinate is relative [0, 1] or absolute [0, parentSize.X].
        /// </summary>
        public bool IsSizeXRelative { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the size's Y-coordinate is relative [0, 1] or absolute [0, parentSize.Y].
        /// </summary>
        public bool IsSizeYRelative { get; set; }

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
                    Parent.Center.X + (IsParentOriginXRelative ? ParentOrigin.X * Parent.ScreenSize.X / 2 : ParentOrigin.X),
                    Parent.Center.Y + (IsParentOriginYRelative ? ParentOrigin.Y * Parent.ScreenSize.Y / 2 : ParentOrigin.Y));

                return new Vector2(
                    parentOriginScreenSpace.X - (IsLocalOriginXRelative ? LocalOrigin.X * ScreenSize.X / 2 : LocalOrigin.X),
                    parentOriginScreenSpace.Y - (IsLocalOriginYRelative ? LocalOrigin.Y * ScreenSize.Y / 2 : LocalOrigin.Y));
            }
        }

        /// <inheritdoc />
        public Vector2 ScreenSize
        {
            get
            {
                return new Vector2(
                    IsSizeXRelative ? Parent.ScreenSize.X * Size.X : Size.X,
                    IsSizeYRelative ? Parent.ScreenSize.Y * Size.Y : Size.Y);
            }
        }

        /// <summary>
        /// Action to be invoked whenever an element property is updated. Used to e.g. update OpenGL buffers when the element changes.
        /// </summary>
        internal Action<GuiElement> Updated { get; set; }

        // TODO: handlers - onclick, onmouseover, onmouseout etc
    }
}
