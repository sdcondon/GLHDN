namespace GLHDN.Views.Renderables.Gui
{
    using GLHDN.ReactiveBuffers;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Numerics;

    /// <summary>
    /// 
    /// </summary>
    public class PanelElement : Element, IElementParent
    {
        private Vector4 color;
        private float borderWidth;
        private SubElementCollection subElements;

        public PanelElement(
            Dimensions parentOrigin,
            Dimensions localOrigin,
            Dimensions relativeSize,
            Vector4 color,
            float borderWidth): base(parentOrigin, localOrigin, relativeSize)
        {
            this.color = color;
            this.borderWidth = borderWidth;
            subElements = new SubElementCollection(this);
        }

        /// <summary>
        /// Gets or sets the background color of the element.
        /// </summary>
        public Vector4 Color
        {
            get => color;
            set
            {
                color = value;
                OnPropertyChanged(nameof(Color));
            }
        }

        /// <summary>
        /// Gets or sets the width of the border of the element.
        /// </summary>
        public float BorderWidth
        {
            get => borderWidth;
            set
            {
                borderWidth = value;
                OnPropertyChanged(nameof(BorderWidth));
            }
        }

        /// <inheritdoc />
        public override IList<GuiVertex> Vertices => new[]
        {
            new GuiVertex(PosTL, Color, PosBL, Size, BorderWidth),
            new GuiVertex(PosTR, Color, PosBL, Size, BorderWidth),
            new GuiVertex(PosBL, Color, PosBL, Size, BorderWidth),
            new GuiVertex(PosBR, Color, PosBL, Size, BorderWidth)
        };

        // TODO: handlers? - onclick, onmouseover, onmouseout etc

        public ICollection<Element> SubElements => subElements;

        /// <inheritdoc /> from IElementParent
        IObservable<IObservable<Element>> IElementParent.SubElements => subElements.ToObservable<Element, Element>(a => a);

        /// <inheritdoc />
        public override void OnPropertyChanged(string propertyName)
        {
            foreach (var subElement in SubElements)
            {
                subElement.OnPropertyChanged(nameof(Parent));
            }
            base.OnPropertyChanged(propertyName);
        }
    }
}
