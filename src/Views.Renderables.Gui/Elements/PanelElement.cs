namespace GLHDN.Views.Renderables.Gui
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Numerics;

    /// <summary>
    /// 
    /// </summary>
    public class PanelElement : GuiElement
    {
        /// <summary>
        /// Gets or sets the background color of the element.
        /// </summary>
        public Vector4 Color { get; set; }

        /// <summary>
        /// Gets or sets the width of the border of the element.
        /// </summary>
        public float BorderWidth { get; set; }

        /// <inheritdoc />
        public override GuiVertex[] Vertices
        {
            get
            {
                var vertices = new List<GuiVertex>()
                {
                    new GuiVertex(this.PosTL, Color, new Vector2(0, Size_ScreenSpace.Y), Size_ScreenSpace, BorderWidth),
                    new GuiVertex(this.PosTR, Color, Size_ScreenSpace, Size_ScreenSpace, BorderWidth),
                    new GuiVertex(this.PosBL, Color, Vector2.Zero, Size_ScreenSpace, BorderWidth),
                    new GuiVertex(this.PosBR, Color, new Vector2(Size_ScreenSpace.X, 0), Size_ScreenSpace, BorderWidth)
                };

                return vertices.ToArray();
            }
        }

        public override event PropertyChangedEventHandler PropertyChanged;

        // TODO: handlers? - onclick, onmouseover, onmouseout etc
    }
}
