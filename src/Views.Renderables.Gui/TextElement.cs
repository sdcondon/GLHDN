namespace GLHDN.Views.Renderables.Gui
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Text;

    public class TextElement : Element
    {
        public static readonly Lazy<Font> font = new Lazy<Font>(() => new Font("Fonts\\Inconsolata\\Inconsolata-Regular.ttf"));

        private Vector4 color;
        private string content;

        public TextElement(
            Dimensions parentOrigin,
            Dimensions localOrigin,
            Dimensions relativeSize,
            Vector4 color,
            string content = "")
            : base(parentOrigin, localOrigin, relativeSize)
        {
            this.color = color;
            this.content = content;
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
                OnPropertyChanged(nameof(Color));
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
                OnPropertyChanged(nameof(Content));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the text of this element is right-aligned.
        /// </summary>
        public bool RightAligned { get; set; }

        /// <inheritdoc />
        public override IList<GuiVertex> Vertices
        {
            get
            {
                var scale = 1f;
                var vertices = new List<GuiVertex>();
                var position = this.PosTL;
                foreach (var line in GetLines(scale))
                {
                    position = new Vector2(this.PosTL.X, position.Y - font.Value.LineHeight/64);

                    foreach (var c in line)
                    {
                        var glyphInfo = AddChar(c, position, vertices, scale);
                        position.X += glyphInfo.Advance * scale; // Advance cursor for next glyph
                    }
                }

                return vertices.ToArray();
            }
        }

        private IEnumerable<string> GetLines(float scale)
        {
            var currentLine = new StringBuilder();
            var lineLength = 0f;

            foreach (var c in Content)
            {
                var glyph = font.Value[c];

                if (c == '\n' || (lineLength + glyph.Bearing.X * scale + glyph.Size.X > this.Size.X && currentLine.Length > 0))
                {
                    yield return currentLine.ToString();
                    currentLine = new StringBuilder();
                    lineLength = 0;
                }

                if (c != '\n')
                {
                    currentLine.Append(c);
                    lineLength += glyph.Advance * scale;
                }
            }

            yield return currentLine.ToString();
        }

        private Font.GlyphInfo AddChar(char c, Vector2 position, List<GuiVertex> vertices, float scale)
        {
            var glyphInfo = font.Value[c];

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

            return glyphInfo;
        }
    }
}
