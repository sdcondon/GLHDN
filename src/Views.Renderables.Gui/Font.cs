using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using OpenGL;
using SharpFont;

namespace GLHDN.Views.Renderables.Gui
{
    /// <summary>
    /// https://www.freetype.org/freetype2/docs/tutorial/step1.html and
    /// https://learnopengl.com/In-Practice/Text-Rendering
    /// </summary>
    public sealed class Font : IDisposable
    {
        private static readonly Library sharpFont = new Library();

        private readonly Face face;
        private readonly Dictionary<char, uint> glyphIndicesByChar = new Dictionary<char, uint>();
        private readonly object glyphsLock = new object();
        private GlyphInfo[] glyphs;

        /// <summary>
        /// Initializes a new instance of the <see cref="Font"/> class.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="pixelSize"></param>
        public Font(string filePath, uint pixelSize = 16)
        {
            this.face = new Face(sharpFont, filePath);
            this.face.SetPixelSizes(0, pixelSize);
            this.face.SelectCharmap(Encoding.Unicode);
            // this.face.SetCharSize(0, 16 * 64, 300, 300);

            // Establish maximum glyph dimensions;
            for (uint i = 0; i < face.GlyphCount; i++)
            {
                face.LoadGlyph(i, LoadFlags.Default, LoadTarget.Normal);
                Max = (Math.Max(Max.Width, face.Glyph.Metrics.Width.ToInt32()), Math.Max(Max.Height, face.Glyph.Metrics.Height.ToInt32()));
            }
        }

        /// <summary>
        /// Gets the Id of the (2D array) texture that holds the glyphs for this font.
        /// </summary>
        public uint TextureId { get; private set; }

        public short LineHeight => face.Height;

        public (int Width, int Height) Max { get; }

        public GlyphInfo this[char c]
        {
            get
            {
                // Lazily load glyphs on first access because its easy.
                // Would be better to load as soon as device context is available.
                if (glyphs == null)
                {
                    lock (glyphsLock)
                    {
                        if (glyphs == null)
                        {
                            LoadGlyphs();
                        }
                    }
                }

                // Apparently some fonts have char entries for a LOT of characters,
                // so do this lazily rather than looping at ctor time
                if (!glyphIndicesByChar.ContainsKey(c))
                {
                    glyphIndicesByChar[c] = face.GetCharIndex(c);
                }

                return glyphs[glyphIndicesByChar[c]];
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            face?.Dispose();
        }

        private void LoadGlyphs()
        {
            if (DeviceContext.GetCurrentContext() == IntPtr.Zero)
            {
                throw new InvalidOperationException("No current OpenGL context!");
            }

            this.glyphs = new GlyphInfo[face.GlyphCount];

            // Create texture array to store glyphs
            // TODO: all direct Gl usage should be in core - add a GlTexture(2dArray?) class.
            Gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            this.TextureId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2dArray, this.TextureId);
            Gl.TexStorage3D(
                target: TextureTarget.Texture2dArray,
                levels: 1,
                internalformat: InternalFormat.Alpha8,
                width: Max.Width,
                height: Max.Height,
                depth: face.GlyphCount);
            Gl.TexParameter(TextureTarget.Texture2dArray, TextureParameterName.TextureWrapS, Gl.CLAMP_TO_EDGE);
            Gl.TexParameter(TextureTarget.Texture2dArray, TextureParameterName.TextureWrapT, Gl.CLAMP_TO_EDGE);
            Gl.TexParameter(TextureTarget.Texture2dArray, TextureParameterName.TextureMinFilter, Gl.LINEAR);
            Gl.TexParameter(TextureTarget.Texture2dArray, TextureParameterName.TextureMagFilter, Gl.LINEAR);

            // Eagerly load all glyphs
            for (uint i = 0; i < face.GlyphCount; i++)
            {
                LoadGlyph(i);
            }
        }

        private void LoadGlyph(uint glyphIndex)
        {
            face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
            face.Glyph.RenderGlyph(RenderMode.Normal);

            if (face.Glyph.Bitmap.Buffer != IntPtr.Zero)
            {
                Gl.TexSubImage3D(
                    target: TextureTarget.Texture2dArray,
                    level: 0,
                    xoffset: 0,
                    yoffset: 0,
                    zoffset: (int)glyphIndex,
                    width: face.Glyph.Bitmap.Width,
                    height: face.Glyph.Bitmap.Rows,
                    depth: 1,
                    format: PixelFormat.Alpha,
                    type: PixelType.UnsignedByte,
                    pixels: face.Glyph.Bitmap.BufferData);
            }

            this.glyphs[glyphIndex] = new GlyphInfo(
                glyphIndex,
                new Vector2(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows),
                new Vector2(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
                (uint)(double)face.Glyph.Advance.X);
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
