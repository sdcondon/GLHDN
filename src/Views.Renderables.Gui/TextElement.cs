namespace GLHDN.Views.Renderables.Gui
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Numerics;

    public class TextElement : Element
    {
        public static readonly Font font = new Font("Fonts\\Inconsolata\\Inconsolata-Regular.ttf");

        private Vector4 color;
        private string content;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextElement"/> class.
        /// </summary>
        /// <param name="content"></param>
        public TextElement(string content)
        {
            this.Content = content;
        }

        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        public Vector4 Color
        {
            get => color;
            set
            {
                color = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color)));
            }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Content
        {
            get => content;
            set
            {
                content = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Content)));
            }
        }

        /// <inheritdoc />
        public override GuiVertex[] Vertices
        {
            get
            {
                var vertices = new List<GuiVertex>();

                var position = this.PosBL;
                var scale = 1f;
                foreach (var c in Content)
                {
                    var glyphInfo = font[c];
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

                    // Now advance cursor for next glyph
                    position.X += glyphInfo.Advance * scale;
                }

                return vertices.ToArray();
            }
        }

        public override event PropertyChangedEventHandler PropertyChanged;
    }
}
