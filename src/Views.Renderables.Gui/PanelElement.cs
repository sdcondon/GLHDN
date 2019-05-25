namespace GLHDN.Views.Renderables.Gui
{
    using System.Collections.Generic;
    using System.Numerics;

    /// <summary>
    /// 
    /// </summary>
    public class PanelElement : Element, IElementParent
    {
        private Vector4 color;
        private float borderWidth;

        public PanelElement(Layout layout, Vector4 color, float borderWidth)
            : base(layout)
        {
            this.color = color;
            this.borderWidth = borderWidth;
            this.SubElements = new ElementCollection(this);
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

        /// <inheritdoc /> from IElementParent
        public ElementCollection SubElements { get; }

        /// <inheritdoc />
        internal override void OnPropertyChanged(string propertyName)
        {
            foreach (var subElement in SubElements)
            {
                subElement.OnPropertyChanged(nameof(Parent));
            }
            base.OnPropertyChanged(propertyName);
        }
    }
}
