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
        private Layout layout;

        /// <summary>
        /// Initializes a new instance of the <see cref="Element"/> class.
        /// </summary>
        /// <param name="layout"></param>
        public Element(Layout layout)
        {
            this.layout = layout;
        }

        /// <summary>
        /// Gets the parent of this element.
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
        /// Gets or sets the object that controls the positioning and size of this element.
        /// </summary>
        public Layout Layout
        {
            get => layout;
            set
            {
                layout = value;
                OnPropertyChanged(nameof(Layout));
            }
        }

        /// <summary>
        /// Gets the position of the center of the element, in screen space.
        /// </summary>
        public Vector2 Center => Layout.GetCenter(this);

        /// <summary>
        /// Gets the size of the element, in screen space.
        /// </summary>
        public Vector2 Size => Layout.GetSize(this);

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

        /// <inheritdoc /> from INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<Vector2> Clicked; 

        public bool Contains(Vector2 position)
        {
            return position.X > this.PosTL.X
                && position.X < this.PosTR.X
                && position.Y < this.PosTL.Y
                && position.Y > this.PosBL.Y;
        }

        // TODO: Instead of this being public, IElementParent should inherit INotifyPropertyChanged
        internal virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void OnClicked(Vector2 position)
        {
            Clicked?.Invoke(this, position);
        }
    }
}
