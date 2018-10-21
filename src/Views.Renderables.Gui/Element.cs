namespace GLHDN.Views.Renderables.Gui
{
    using System.ComponentModel;
    using System.Numerics;

    /// <summary>
    /// Base class for GUI elements. Provides for a nested element hierarchy, with elements being placed relative to their parents.
    /// </summary>
    /// <remarks>
    /// Shoehorns quite a bit into one class, reducing flexibility. Might want to refactor at some point.
    /// </remarks>
    public abstract class Element : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the parent element of this element.
        /// </summary>
        public IElementParent Parent { get; internal set; }

        /// <summary>
        /// Gets or sets the position in parent-space of the local origin.
        /// </summary>
        public Dimensions ParentOrigin { get; set; }

        /// <summary>
        /// Gets or sets the position relative to the center of the element that will be placed at the parent origin.
        /// </summary>
        public Dimensions LocalOrigin { get; set; }

        /// <summary>
        /// Gets or sets the size of the element.
        /// </summary>
        public Dimensions Size { get; set; }

        /// <summary>
        /// Gets the position of the center of the element, in screen space.
        /// </summary>
        public virtual Vector2 Center_ScreenSpace
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

        /// <summary>
        /// Gets the size of the element, in screen space (i.e. pixels).
        /// </summary>
        public virtual Vector2 Size_ScreenSpace
        {
            get
            {
                return new Vector2(
                    Size.IsXRelative ? Parent.Size_ScreenSpace.X * Size.X : Size.X,
                    Size.IsYRelative ? Parent.Size_ScreenSpace.Y * Size.Y : Size.Y);
            }
        }

        public Vector2 PosBL => this.Center_ScreenSpace - this.Size_ScreenSpace / 2;

        public Vector2 PosBR => new Vector2(this.Center_ScreenSpace.X + this.Size_ScreenSpace.X / 2, this.Center_ScreenSpace.Y - this.Size_ScreenSpace.Y / 2);

        public Vector2 PosTL => new Vector2(this.Center_ScreenSpace.X - this.Size_ScreenSpace.X / 2, this.Center_ScreenSpace.Y + this.Size_ScreenSpace.Y / 2);

        public Vector2 PosTR => this.Center_ScreenSpace + this.Size_ScreenSpace / 2;

        public abstract GuiVertex[] Vertices { get; }

        public abstract event PropertyChangedEventHandler PropertyChanged;
    }
}
