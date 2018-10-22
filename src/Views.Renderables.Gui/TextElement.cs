namespace GLHDN.Views.Renderables.Gui
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Numerics;

    public class TextElement : Element
    {
        private readonly Font font;

        public TextElement(string fontPath, string content)
        {
            this.font = new Font(fontPath);
            this.Content = content;
        }

        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        public Vector4 Color { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Content { get; set; }

        /// <inheritdoc />
        public override GuiVertex[] Vertices
        {
            get
            {
                var vertices = new List<GuiVertex>();

                // TODO: add vertices for chars
                var position = this.PosBL;
                var scale = 1f;
                foreach (var c in Content)
                {
                    var glyphInfo = this.font[c];
                    var charSize = new Vector2(glyphInfo.Size.X * scale, glyphInfo.Size.Y * scale);
                    var charPosBL = new Vector2(position.X + glyphInfo.Bearing.X * scale, position.Y + (glyphInfo.Bearing.Y - glyphInfo.Size.Y) * scale);
                    var charPosBR = charPosBL + Vector2.UnitX * charSize.X;
                    var charPosTL = charPosBL + Vector2.UnitY * charSize.Y;
                    var charPosTR = charPosBL + charSize;

                    vertices.AddRange(new[]
                    {
                        new GuiVertex(charPosTL, Color, charPosBL, charSize, (int)c),
                        new GuiVertex(charPosTR, Color, charPosBL, charSize, (int)c),
                        new GuiVertex(charPosBL, Color, charPosBL, charSize, (int)c),
                        new GuiVertex(charPosBR, Color, charPosBL, charSize, (int)c)
                    });

                    // Now advance cursors for next glyph (note that advance is number of 1/64 pixels)
                    position.X += (glyphInfo.Advance >> 6) * scale; // Bitshift by 6 to get value in pixels (2^6 = 64)
                }

                return vertices.ToArray();
            }
        }

        public override event PropertyChangedEventHandler PropertyChanged;
    }
}
