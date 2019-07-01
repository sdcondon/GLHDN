namespace GLHDN.Views.Renderables.Gui
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text;

    public class TextElement : ElementBase
    {
        public static readonly Lazy<Font> font = new Lazy<Font>(() => new Font("Fonts\\Inconsolata\\Inconsolata-Regular.ttf"));

        private Vector4 color;
        private string content;
        private float horizontalAlignment;
        private float verticalAlignment;

        public TextElement(Layout layout, Vector4 color, string content = "")
            : base(layout)
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
        /// Gets or sets the horizontal aligment of the text within the element.
        /// </summary>
        public float HorizontalAlignment
        {
            get => horizontalAlignment;
            set
            {
                horizontalAlignment = value;
                OnPropertyChanged(nameof(HorizontalAlignment));
            }
        }

        /// <summary>
        /// Gets or sets the vertical aligment of the text within the element.
        /// </summary>
        public float VerticalAlignment
        {
            get => verticalAlignment;
            set
            {
                verticalAlignment = value;
                OnPropertyChanged(nameof(VerticalAlignment));
            }
        }

        /// <inheritdoc />
        public override IList<Vertex> Vertices
        {
            get
            {
                var scale = 1f;
                var vertices = new List<Vertex>();
                var lineHeight = font.Value.LineHeight / 64;
                var lines = GetLines(scale);
                var lineBottomLeft = this.PosTL - Vector2.UnitY * ((this.Size.Y - (lineHeight * lines.Count())) * verticalAlignment);
                foreach (var line in lines)
                {
                    lineBottomLeft.Y -= lineHeight;

                    if (line.Count > 0)
                    {
                        var lineSize = line[line.Count - 1].position - line[0].position;
                        lineBottomLeft.X = this.PosTL.X + ((this.Size.X - lineSize.X) * horizontalAlignment);

                        foreach (var v in line)
                        {
                            vertices.Add(new Vertex(
                                v.position + lineBottomLeft,
                                v.color,
                                v.position + lineBottomLeft - v.elementPosition,
                                v.elementSize,
                                v.borderWidth));
                        }
                    }
                }

                return vertices.ToArray();
            }
        }

        private IEnumerable<IList<Vertex>> GetLines(float scale)
        {
            List<Vertex> currentLine;
            float lineLength = 0f;

            void startNewLine()
            {
                currentLine = new List<Vertex>();
                lineLength = 0f;
            }

            startNewLine();

            foreach (var c in Content)
            {
                var glyph = font.Value[c];

                if (c == '\n' || (lineLength + glyph.Bearing.X * scale + glyph.Size.X > this.Size.X && currentLine.Count > 0))
                {
                    yield return currentLine;
                    startNewLine();
                }

                if (c != '\n')
                {
                    AddChar(c, new Vector2(lineLength, 0f), currentLine, scale);
                    lineLength += glyph.Advance * scale;
                }
            }

            yield return currentLine;
        }

        private void AddChar(char c, Vector2 position, List<Vertex> vertices, float scale)
        {
            var glyphInfo = font.Value[c];

            var charSize = new Vector2(glyphInfo.Size.X * scale, glyphInfo.Size.Y * scale);
            var charPosBL = new Vector2(position.X + glyphInfo.Bearing.X * scale, position.Y + (glyphInfo.Bearing.Y - glyphInfo.Size.Y) * scale);
            var charPosBR = charPosBL + Vector2.UnitX * charSize.X;
            var charPosTL = charPosBL + Vector2.UnitY * charSize.Y;
            var charPosTR = charPosBL + charSize;

            vertices.AddRange(new[]
            {
                new Vertex(charPosTL, Color, charPosBL, charSize, (int)c),
                new Vertex(charPosTR, Color, charPosBL, charSize, (int)c),
                new Vertex(charPosBL, Color, charPosBL, charSize, (int)c),
                new Vertex(charPosBR, Color, charPosBL, charSize, (int)c)
            });
        }
    }
}
