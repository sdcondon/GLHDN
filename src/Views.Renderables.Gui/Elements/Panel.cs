using System.Collections.Generic;
using System.Numerics;

namespace GLHDN.Views.Renderables.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class Panel : ContainerElementBase
    {
        private Vector4 color;

        public Panel(Layout layout, Vector4 color)
            : base(layout)
        {
            this.color = color;
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

        /// <inheritdoc />
        public override IList<Vertex> Vertices => new[]
        {
            new Vertex(PosTL, Color),
            new Vertex(PosTR, Color),
            new Vertex(PosBL, Color),
            new Vertex(PosBR, Color)
        };
    }
}
