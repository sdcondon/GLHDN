namespace GLHDN.Views.Renderables.Gui.Elements
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Numerics;

    public class Text : IGuiElement
    {
        public IGuiElement Parent { get; internal set; }

        /// <summary>
        /// Gets or sets the position in parent-space of the local origin.
        /// </summary>
        public Dimensions ParentOrigin { get; set; }

        /// <summary>
        /// Gets or sets the position relative to the control that will be placed at the parent origin.
        /// </summary>
        public Dimensions LocalOrigin { get; set; }

        /// <summary>
        /// Gets or sets the size of the element.
        /// </summary>
        public Dimensions Size { get; set; }

        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        public Vector4 Color { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Content { get; set; }

        /// <inheritdoc />
        public Vector2 Center_ScreenSpace
        {
            get
            {
                var parentOriginScreenSpace = new Vector2(
                    Parent.Center_ScreenSpace.X + (ParentOrigin.IsXRelative ? ParentOrigin.X * Parent.Size_ScreenSpace.X / 2 : ParentOrigin.X),
                    Parent.Center_ScreenSpace.Y + (ParentOrigin.IsYRelative ? ParentOrigin.Y * Parent.Size_ScreenSpace.Y / 2 : ParentOrigin.Y));

                return new Vector2(
                    parentOriginScreenSpace.X - (LocalOrigin.IsXRelative ? LocalOrigin.X * Size_ScreenSpace.X / 2 : LocalOrigin.X),
                    parentOriginScreenSpace.Y - (LocalOrigin.IsYRelative ? LocalOrigin.Y * Size_ScreenSpace.Y / 2 : LocalOrigin.Y));
            }
        }

        /// <inheritdoc />
        public Vector2 Size_ScreenSpace
        {
            get
            {
                return new Vector2(
                    Size.IsXRelative ? Parent.Size_ScreenSpace.X * Size.X : Size.X,
                    Size.IsYRelative ? Parent.Size_ScreenSpace.Y * Size.Y : Size.Y);
            }
        }

        /// <inheritdoc />
        public GuiVertex[] Vertices
        {
            get
            {
                var vertices = new List<GuiVertex>();

                // TODO: add vertices for chars
                var font = new Font();
                var position = this.GetPosBL();
                var scale = 1f;
                foreach (var c in Content)
                {
                    var ch = font[c];
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
