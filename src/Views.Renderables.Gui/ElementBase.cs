using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;

namespace GLHDN.Views.Renderables.Gui
{
    /// <summary>
    /// Base class for GUI elements. Provides for a nested element hierarchy, with elements being placed relative to their parents.
    /// </summary>
    public abstract class ElementBase : INotifyPropertyChanged, IDisposable 
    {
        private IElementParent parent;
        private Layout layout;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementBase"/> class.
        /// </summary>
        /// <param name="layout"></param>
        public ElementBase(Layout layout)
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
                if (parent != null)
                {
                    parent.PropertyChanged -= Parent_PropertyChanged;
                    parent.Clicked -= Parent_Clicked;
                }

                parent = value;
                OnPropertyChanged(nameof(Parent));

                parent.PropertyChanged += Parent_PropertyChanged;
                parent.Clicked += Parent_Clicked;
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
        /// Gets the list of vertices to be rendered for this GUI element (not including any children).
        /// </summary>
        public virtual IList<Vertex> Vertices { get; } = new Vertex[0];

        /// <inheritdoc /> from INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
        public virtual void Dispose()
        {
            if (parent != null)
            {
                parent.PropertyChanged -= Parent_PropertyChanged;
                parent.Clicked -= Parent_Clicked;
            }

            GC.SuppressFinalize(this);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnClicked(Vector2 position)
        {
        }

        private void Parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Parent));
        }

        private void Parent_Clicked(object sender, Vector2 e)
        {
            var isClickInBounds = e.X > this.PosTL.X
                && e.X < this.PosTR.X
                && e.Y < this.PosTL.Y
                && e.Y > this.PosBL.Y;

            if (isClickInBounds)
            {
                OnClicked(e);
            }
        }
    }
}
