namespace GLHDN.Views.Renderables.Gui
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Numerics;
    using OpenGL;
    using SharpFont;

    /// <summary>
    /// https://www.freetype.org/freetype2/docs/tutorial/step1.html and
    /// https://learnopengl.com/In-Practice/Text-Rendering
    /// </summary>
    public sealed class Font : IDisposable
    {
        private static readonly Library sharpFont = new Library();

        private readonly Dictionary<char, GlyphInfo> glyphs = new Dictionary<char, GlyphInfo>();
        private readonly Face face;

        /// <summary>
        /// Initializes a new instance of the <see cref="Font"/> class.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="pixelSize"></param>
        public Font(string filePath, uint pixelSize = 16)
        {
            void DebugWriteLine(string msg) => Debug.WriteLine(msg, $"{this}:{nameof(Font)}");

            DebugWriteLine("Constructing new Font object");
            face = new Face(sharpFont, filePath);
            face.SetPixelSizes(0, pixelSize); 
            //face.SetCharSize(0, 16 * 64, 300, 300);

            const int maxChar = 128;
            int maxWidth = 0, maxHeight = 0;
            for (char c = (char)0; c < maxChar; c++)
            {
                face.LoadChar(c, LoadFlags.Default, LoadTarget.Normal);
                face.Glyph.RenderGlyph(RenderMode.Normal);
                maxWidth = Math.Max(maxWidth, face.Glyph.Bitmap.Width);
                maxHeight = Math.Max(maxHeight, face.Glyph.Bitmap.Rows);
            }

            // TODO: really, there should be no direct Gl usage other than in core. add this logic to it
            Gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            this.TextureId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2dArray, this.TextureId);
            Gl.TexStorage3D(
                target: TextureTarget.Texture2dArray,
                levels: 1,
                internalformat: InternalFormat.Alpha8,
                width: maxWidth,
                height: maxHeight,
                depth: maxChar);
            Gl.TexParameter(TextureTarget.Texture2dArray, TextureParameterName.TextureWrapS, Gl.CLAMP_TO_EDGE);
            Gl.TexParameter(TextureTarget.Texture2dArray, TextureParameterName.TextureWrapT, Gl.CLAMP_TO_EDGE);
            Gl.TexParameter(TextureTarget.Texture2dArray, TextureParameterName.TextureMinFilter, Gl.LINEAR);
            Gl.TexParameter(TextureTarget.Texture2dArray, TextureParameterName.TextureMagFilter, Gl.LINEAR);

            // Loop glyphs and pack them
            for (char c = (char)0; c < maxChar; c++)
            {
                face.LoadChar(c, LoadFlags.Default, LoadTarget.Normal);
                face.Glyph.RenderGlyph(RenderMode.Normal);

                if (face.Glyph.Bitmap.Buffer != IntPtr.Zero)
                {
                    Gl.TexSubImage3D(
                        target: TextureTarget.Texture2dArray,
                        level: 0,
                        xoffset: 0,
                        yoffset: 0,
                        zoffset: c,
                        width: face.Glyph.Bitmap.Width,
                        height: face.Glyph.Bitmap.Rows,
                        depth: 1,
                        format: PixelFormat.Alpha,
                        type: PixelType.UnsignedByte,
                        pixels: face.Glyph.Bitmap.BufferData);
                }

                this.glyphs[c] = new GlyphInfo(
                    c,
                    new Vector2(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows),
                    new Vector2(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
                    (uint)(double)face.Glyph.Advance.X);
            }
        }

        /// <summary>
        /// Gets the Id of the (2D array) texture that holds the glyphs for this font.
        /// </summary>
        public uint TextureId { get; private set; }

        public short LineHeight => face.Height;

        public GlyphInfo this[char c] => glyphs[c];

        /// <inheritdoc />
        public void Dispose()
        {
            face?.Dispose();
        }

        public struct GlyphInfo
        {
            public GlyphInfo(uint zOffset, Vector2 size, Vector2 bearing, uint advance)
            {
                ZOffset = zOffset;
                Size = size;
                Bearing = bearing;
                Advance = advance;
            }

            public uint ZOffset { get; }
            public Vector2 Size { get; }
            public Vector2 Bearing { get; }
            public uint Advance { get; }
        }
    }
}
