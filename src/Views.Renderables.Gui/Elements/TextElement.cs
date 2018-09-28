﻿namespace GLHDN.Views.Renderables.Gui.Elements
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Numerics;

    public class TextElement : GuiElement
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
                var position = this.GetPosBL();
                var scale = 1f;
                foreach (var c in Content)
                {
                    var ch = this.font[c];
                    var xpos = position.X + ch.Bearing.X * scale;
                    var ypos = position.Y + (ch.Bearing.Y - ch.Size.Y) * scale;
                    var w = ch.Size.X * scale;
                    var h = ch.Size.Y * scale;

                    //vertices.AddRange(new[]
                    //{
                    //    new GuiVertex(new Vector2(xpos, ypos + h), Color, new Vector2(0, ScreenSize.Y), ScreenSize, BorderWidth),
                    //    new GuiVertex(new Vector2(xpos + w, ypos + h), Color, ScreenSize, ScreenSize, BorderWidth),
                    //    new GuiVertex(new Vector2(xpos, ypos), Color, Vector2.Zero, ScreenSize, BorderWidth),
                    //    new GuiVertex(new Vector2(xpos + w, ypos), Color, new Vector2(ScreenSize.X, 0), ScreenSize, BorderWidth)
                    //});

                    // Now advance cursors for next glyph (note that advance is number of 1/64 pixels)
                    position.X += (ch.Advance >> 6) * scale; // Bitshift by 6 to get value in pixels (2^6 = 64)
                }

                return vertices.ToArray();
            }
        }

        public override event PropertyChangedEventHandler PropertyChanged;
    }
}