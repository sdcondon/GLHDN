namespace GLHDN.Views.Renderables.Gui
{
    using System.ComponentModel;
    using System.Numerics;

    /// <summary>
    /// Base class for GUI elements. Provides for a nested element hierarchy, with elements being placed relative to their parents.
    /// </summary>
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
        /// Gets or sets the size of the element in relation to its parent.
        /// </summary>
        public Dimensions RelativeSize { get; set; }

        /// <summary>
        /// Gets the position of the center of the element, in screen space.
        /// </summary>
        public Vector2 Center
        {
            get
            {
                var parentOriginScreenSpace = new Vector2(
                    Parent.Center.X + (ParentOrigin.IsXRelative ? ParentOrigin.X * Parent.Size.X / 2 : ParentOrigin.X),
                    Parent.Center.Y + (ParentOrigin.IsYRelative ? ParentOrigin.Y * Parent.Size.Y / 2 : ParentOrigin.Y));

                return new Vector2(
                    parentOriginScreenSpace.X - (LocalOrigin.IsXRelative ? LocalOrigin.X * Size.X / 2 : LocalOrigin.X),
                    parentOriginScreenSpace.Y - (LocalOrigin.IsYRelative ? LocalOrigin.Y * Size.Y / 2 : LocalOrigin.Y));
            }
        }

        /// <summary>
        /// Gets the size of the element, in screen space (i.e. pixels).
        /// </summary>
        public Vector2 Size
        {
            get
            {
                return new Vector2(
                    RelativeSize.IsXRelative ? Parent.Size.X * RelativeSize.X : RelativeSize.X,
                    RelativeSize.IsYRelative ? Parent.Size.Y * RelativeSize.Y : RelativeSize.Y);
            }
        }

        public Vector2 PosBL => this.Center - this.Size / 2;

        public Vector2 PosBR => new Vector2(this.Center.X + this.Size.X / 2, this.Center.Y - this.Size.Y / 2);

        public Vector2 PosTL => new Vector2(this.Center.X - this.Size.X / 2, this.Center.Y + this.Size.Y / 2);

        public Vector2 PosTR => this.Center + this.Size / 2;

        public abstract GuiVertex[] Vertices { get; }

        public abstract event PropertyChangedEventHandler PropertyChanged;
    }
}
