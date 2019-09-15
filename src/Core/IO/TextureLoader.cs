namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Simple texture loader nabbed from an OpenGL tutorial.
    /// </summary>
    public static class TextureLoader
    {
        private static readonly Dictionary<string, InternalFormat> DDSPixelFormats = new Dictionary<string, InternalFormat>()
        {
            { "DXT1", InternalFormat.CompressedRgbaS3tcDxt1Ext },
            { "DXT3", InternalFormat.CompressedRgbaS3tcDxt3Ext },
            { "DXT5", InternalFormat.CompressedRgbaS3tcDxt5Ext },
        };

        /// <summary>
        /// Loads a DDS image from a given file path.
        /// </summary>
        /// <param name="filePath">The file path to load the image from.</param>
        /// <returns>The OpenGL texture ID that the image has been loaded into.</returns>
        public static uint LoadDDS(string filePath)
        {
            uint height;
            uint width;
            uint mipMapCount;
            InternalFormat format;
            byte[] buffer;
            using (var file = File.Open(filePath, FileMode.Open))
            {
                // Read file header
                var header = new byte[128];
                file.Read(header, 0, 128);

                if (Encoding.ASCII.GetString(header, 0, 4) != "DDS ")
                {
                    throw new ArgumentException("Specified file is not a DDS file", nameof(filePath));
                }

                height = BitConverter.ToUInt32(header, 12);
                width = BitConverter.ToUInt32(header, 16);
                var linearSize = BitConverter.ToUInt32(header, 20);
                mipMapCount = BitConverter.ToUInt32(header, 28);
                var fourCC = Encoding.ASCII.GetString(header, 84, 4);
                if (!DDSPixelFormats.TryGetValue(Encoding.ASCII.GetString(header, 84, 4), out format))
                {
                    throw new ArgumentException($"Specified file uses unsupported internal format {fourCC}", nameof(filePath));
                }

                ////uint components = (fourCC == FOURCC_DXT1) ? 3u : 4u;

                // Read the rest of the file
                var bufferSize = (int)(mipMapCount > 1 ? linearSize * 2 : linearSize);
                buffer = new byte[bufferSize];
                file.Read(buffer, 0, bufferSize);
            }

            // Create OpenGL texture
            var textureId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, textureId); // "Bind" the newly created texture: all future texture functions will modify this texture
            Gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            // Load the mipmaps
            uint blockSize = (format == InternalFormat.CompressedRgbaS3tcDxt1Ext) ? 8u : 16u;
            uint offset = 0;
            for (int level = 0; level < mipMapCount && (width > 0 || height > 0); ++level)
            {
                uint levelSize = ((width + 3) / 4) * ((height + 3) / 4) * blockSize;

                var levelBuffer = new byte[levelSize];
                Array.Copy(buffer, offset, levelBuffer, 0, levelSize);
                Gl.CompressedTexImage2D(TextureTarget.Texture2d, level, format, (int)width, (int)height, 0, (int)levelSize, levelBuffer);

                offset += levelSize;
                width /= 2;
                height /= 2;

                // Deal with non-power-of-two textures. This code is not included in the webpage to reduce clutter.
                width = Math.Max(width, 1);
                height = Math.Max(height, 1);
            }

            return textureId;
        }

        /// <summary>
        /// Loads a (24bpp) BMP image from a given file path.
        /// </summary>
        /// <param name="filePath">The file path to load the image from.</param>
        /// <returns>The OpenGL texture ID that the image has been loaded into.</returns>
        /// <remarks>
        /// Also see https://en.wikipedia.org/wiki/BMP_file_format.
        /// </remarks>
        public static uint LoadBMP(string filePath)
        {
            uint dataPos; // Position in the file where the actual data begins
            int width, height;
            uint imageSize; // = width*height*3
            byte[] data; // Actual RGB data
            using (var file = File.Open(filePath, FileMode.Open))
            {
                // Read file header
                var header = new byte[54];
                file.Read(header, 0, 54);

                if (Encoding.ASCII.GetString(header, 0, 2) != "BM")
                {
                    throw new ArgumentException("Specified file is not a Windows BMP file", nameof(filePath));
                }

                dataPos = BitConverter.ToUInt32(header, 10);
                width = BitConverter.ToInt32(header, 18);
                height = BitConverter.ToInt32(header, 22);

                var bitsPerPixel = BitConverter.ToUInt16(header, 28);
                if (bitsPerPixel != 24)
                {
                    throw new NotSupportedException($"Only 24BPP BMPs are supported - this BMP is {bitsPerPixel}BPP");
                }

                ////var compressionMethod = BitConverter.ToUInt32(header, 30);
                imageSize = BitConverter.ToUInt32(header, 34);

                if (imageSize == 0)
                {
                    imageSize = (uint)width * (uint)height * bitsPerPixel / 8;
                }

                if (dataPos == 0)
                {
                    dataPos = 54; // The BMP header is done that way
                }

                // Read the pixel data
                var rowSize = bitsPerPixel * width / 8;
                var paddedRowSize = ((bitsPerPixel * width + 31) / 32) * 4; // Each row rounded up to multiple of 4 bytes
                var offset = 0;
                data = new byte[imageSize];
                file.Seek(dataPos, SeekOrigin.Begin);
                for (int i = 0; i < height; i++)
                {
                    file.Read(data, offset, rowSize);
                    offset += rowSize;
                    file.Seek(paddedRowSize - rowSize, SeekOrigin.Current);
                }
            }

            var textureId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, textureId); // "Bind" the newly created texture : all future texture functions will modify this texture
            Gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            // Give the image to OpenGL
            Gl.TexImage2D(
                TextureTarget.Texture2d,
                0,
                InternalFormat.Rgb,
                (int)width,
                (int)height,
                0,
                PixelFormat.Bgr,
                PixelType.UnsignedByte,
                data);

            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.NEAREST);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.NEAREST);

            return textureId;
        }
    }
}
