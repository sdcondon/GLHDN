namespace GLHDN.Views.Renderables.Gui
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Numerics;

    /// <summary>
    /// Base class for GUI elements. Provides for a nested element hierarchy, with elements being placed relative to their parents.
    /// </summary>
    public abstract class Element : INotifyPropertyChanged
    {
        private IElementParent parent;
        private Dimensions parentOrigin;
        private Dimensions localOrigin;
        private Dimensions relativeSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="Element"/> class.
        /// </summary>
        /// <param name="parentOrigin"></param>
        /// <param name="localOrigin"></param>
        /// <param name="relativeSize"></param>
        public Element(Dimensions parentOrigin, Dimensions localOrigin, Dimensions relativeSize)
        {
            this.parentOrigin = parentOrigin;
            this.localOrigin = localOrigin;
            this.relativeSize = relativeSize;
        }

        /// <summary>
        /// Gets the parent element of this element.
        /// </summary>
        public IElementParent Parent
        {
            get => parent;
            internal set
            {
                parent = value;
                OnPropertyChanged(nameof(Parent));
            }
        }

        /// <summary>
        /// Gets or sets the position in parent-space of the local origin.
        /// </summary>
        public Dimensions ParentOrigin
        {
            get => parentOrigin;
            set
            {
                parentOrigin = value;
                OnPropertyChanged(nameof(ParentOrigin));
            }
        }

        /// <summary>
        /// Gets or sets the position relative to the center of the element that will be placed at the parent origin.
        /// </summary>
        public Dimensions LocalOrigin
        {
            get => localOrigin;
            set
            {
                localOrigin = value;
                OnPropertyChanged(nameof(LocalOrigin));
            }
        }

        /// <summary>
        /// Gets or sets the size of the element in relation to its parent.
        /// </summary>
        public Dimensions RelativeSize
        {
            get => relativeSize;
            set
            {
                parentOrigin = value;
                OnPropertyChanged(nameof(RelativeSize));
            }
        }

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
        /// Gets the size of the element, in screen space.
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

        /// <summary>
        /// Gets the position of the bottom-left corner of the element, in screen space.
        /// </summary>
        public Vector2 PosBL => this.Center - this.Size / 2;

        /// <summary>
        /// Gets the position of the bottom-right corner of the element, in screen space.
        /// </summary>
        public Vector2 PosBR => new Vector2(this.Center.X + this.Size.X / 2, this.Center.Y - this.Size.Y / 2);

        /// <summary>
        /// Gets the position of the top-left corner of the element, in screen space.
        /// </summary>
        public Vector2 PosTL => new Vector2(this.Center.X - this.Size.X / 2, this.Center.Y + this.Size.Y / 2);

        /// <summary>
        /// Gets the position of the top-right corner of the element, in screen space.
        /// </summary>
        public Vector2 PosTR => this.Center + this.Size / 2;

        /// <summary>
        /// Gets the list of vertices to be rendered for this GUI element.
        /// </summary>
        public abstract IList<GuiVertex> Vertices { get; }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<Vector2> Clicked; 

        // TODO: Instead of this being public, IElementParent should inherit INotifyPropertyChanged
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void OnClicked(Vector2 position)
        {
            Clicked?.Invoke(this, position);
        }
    }
}
