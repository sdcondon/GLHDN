namespace GLHDN.Views.Renderables.Gui
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Numerics;

    /// <summary>
    /// 
    /// </summary>
    public class Panel : IGuiElement
    {
        public Panel(IGuiElement parent)
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
        public Vector2 Center_ScreenSpace
        {
            get
            {
                var parentOriginScreenSpace = new Vector2(
                    Parent.Center_ScreenSpace.X + (ParentOrigin.IsXRelative ? ParentOrigin.X * Parent.Size_ScreenSpace.X / 2 : ParentOrigin.X),
                    Parent.Center_ScreenSpace.Y + (ParentOrigin.IsYRelative ? ParentOrigin.Y * Parent.Size_ScreenSpace.Y / 2 : ParentOrigin.Y));

                return new Vector2(
                    parentOriginScreenSpace.X - (LocalOrigin.IsXRelative ? LocalOrigin.X * Size_ScreenSpace.X / 2 : LocalOrigin.X),
                    parentOriginScreenSpace.Y - (LocalOrigin.IsYRelative ? LocalOrigin.Y * Size_ScreenSpace.Y / 2 : LocalOrigin.Y));
            }
        }

        /// <inheritdoc />
        public Vector2 Size_ScreenSpace
        {
            get
            {
                return new Vector2(
                    Size.IsXRelative ? Parent.Size_ScreenSpace.X * Size.X : Size.X,
                    Size.IsYRelative ? Parent.Size_ScreenSpace.Y * Size.Y : Size.Y);
            }
        }

        /// <inheritdoc />
        public GuiVertex[] Vertices
        {
            get
            {
                var vertices = new List<GuiVertex>()
                {
                    new GuiVertex(this.GetPosTL(), Color, new Vector2(0, Size_ScreenSpace.Y), Size_ScreenSpace, BorderWidth),
                    new GuiVertex(this.GetPosTR(), Color, Size_ScreenSpace, Size_ScreenSpace, BorderWidth),
                    new GuiVertex(this.GetPosBL(), Color, Vector2.Zero, Size_ScreenSpace, BorderWidth),
                    new GuiVertex(this.GetPosBR(), Color, new Vector2(Size_ScreenSpace.X, 0), Size_ScreenSpace, BorderWidth)
                };

                return vertices.ToArray();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // TODO: handlers? - onclick, onmouseover, onmouseout etc
    }
}
