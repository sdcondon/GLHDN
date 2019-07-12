﻿namespace GLHDN.Views.Renderables.Gui
{
    using System.Collections.Generic;
    using System.Numerics;

    /// <summary>
    /// 
    /// </summary>
    public class PanelElement : ContainerElementBase
    {
        private Vector4 color;
        private float borderWidth;

        public PanelElement(Layout layout, Vector4 color, float borderWidth)
            : base(layout)
        {
            this.color = color;
            this.borderWidth = borderWidth;
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
        public override IList<Vertex> Vertices => new[]
        {
            new Vertex(PosTL, Color, PosBL, Size, BorderWidth),
            new Vertex(PosTR, Color, PosBL, Size, BorderWidth),
            new Vertex(PosBL, Color, PosBL, Size, BorderWidth),
            new Vertex(PosBR, Color, PosBL, Size, BorderWidth)
        };

        // TODO: handlers? - onclick, onmouseover, onmouseout etc
    }
}
