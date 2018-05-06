namespace GLHDN.Views.Renderables.Gui
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using GLHDN.Core;
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
        public Font(
            string filePath = "./Fonts/Inconsolata/Inconsolata-Regular.ttf",
            uint pixelSize = 32)
        {
            var face = new Face(sharpFont, filePath);
            face.SetPixelSizes(0, pixelSize); //or face.SetCharSize(0, 16 * 64, 300, 300);

            // Loop glyphs and pack them
            Gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            for (char c = (char)0; c < 128; c++)
            {
                LoadChar(c);
            }
        }

        public GlyphInfo this[char c]
        {
            get
            {
                return glyphs[c];
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            face?.Dispose();
        }

        private void LoadChar(char c)
        {
            face.LoadChar(c, LoadFlags.Default, LoadTarget.Normal);

            face.Glyph.RenderGlyph(RenderMode.Normal);

            // TODO: no direct Gl usage other than in core. add this logic to it
            var textureId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, textureId);
            Gl.TexImage2D(
                target: TextureTarget.Texture2d,
                level: 0,
                internalFormat: InternalFormat.Red,
                width: face.Glyph.Bitmap.Width,
                height: face.Glyph.Bitmap.Rows,
                border: 0,
                format: PixelFormat.Red,
                type: PixelType.UnsignedByte,
                data: face.Glyph.Bitmap.Buffer);

            Gl.TexParameter(
                TextureTarget.Texture2d,
                TextureParameterName.TextureWrapS,
                Gl.CLAMP_TO_EDGE);
            Gl.TexParameter(
                TextureTarget.Texture2d,
                TextureParameterName.TextureWrapT,
                Gl.CLAMP_TO_EDGE);
            Gl.TexParameter(
                TextureTarget.Texture2d,
                TextureParameterName.TextureMinFilter,
                Gl.LINEAR);
            Gl.TexParameter(
                TextureTarget.Texture2d,
                TextureParameterName.TextureMagFilter,
                Gl.LINEAR);

            this.glyphs[c] = new GlyphInfo(
                textureId,
                new Vector2(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows),
                new Vector2(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
                (uint)(double)face.Glyph.Advance.X);
        }

        public struct GlyphInfo
        {
            public GlyphInfo(
                uint textureId,
                Vector2 size,
                Vector2 bearing,
                uint advance)
            {
                TextureId = textureId;
                Size = size;
                Bearing = bearing;
                Advance = advance;
            }

            public uint TextureId { get; }
            public Vector2 Size { get; }
            public Vector2 Bearing { get; }
            public uint Advance { get; }
        }
    }
}
