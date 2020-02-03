using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;

namespace GLHDN.Views.Renderables.Gui
{
    /// <summary>
    /// Base class for GUI elements that can contain other elements.
    /// </summary>
    public abstract class ContainerElementBase : ElementBase, IElementParent
    {
        public ContainerElementBase(Layout layout)
            : base(layout)
        {
            this.SubElements = new ElementCollection(this);
        }

        /// <inheritdoc /> from IElementParent
        public ElementCollection SubElements { get; }

        /// <inheritdoc />
        public event EventHandler<Vector2> Clicked;

        /// <inheritdoc />
        public override void Dispose()
        {
            // todo: dispose subelements
            base.Dispose();
        }

        /// <inheritdoc />
        protected override void OnClicked(Vector2 position)
        {
            Clicked?.Invoke(this, position);
        }
    }
}
