namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Simple DDS loader nabbed from an OpenGL tutorial.
    /// </summary>
    public static class TextureLoader
    {
        private const uint FOURCC_DXT1 = 0x31545844; // Equivalent to "DXT1" in ASCII
        private const uint FOURCC_DXT3 = 0x33545844; // Equivalent to "DXT3" in ASCII
        private const uint FOURCC_DXT5 = 0x35545844; // Equivalent to "DXT5" in ASCII

        private static readonly Dictionary<string, InternalFormat> DDSPixelFormats = new Dictionary<string, InternalFormat>()
        {
            { "DXT1", InternalFormat.CompressedRgbaS3tcDxt1Ext },
            { "DXT3", InternalFormat.CompressedRgbaS3tcDxt3Ext },
            { "DXT5", InternalFormat.CompressedRgbaS3tcDxt5Ext },
        };

        /// <summary>
        /// Loads a DDS image from a given file path.
        /// </summary>
        /// <param name="imagepath">The file path to load the image from.</param>
        /// <returns>The Open GL texture ID that the image has been loaded into.</returns>
        public static uint LoadDDS(string imagepath)
        {
            uint height;
            uint width;
            uint mipMapCount;
            InternalFormat format;
            byte[] buffer;
            using (var file = File.Open(imagepath, FileMode.Open))
            {
                // Read file header
                var header = new byte[128];
                file.Read(header, 0, 128);

                if (Encoding.ASCII.GetString(header, 0, 4) != "DDS ")
                {
                    throw new ArgumentException("Specified file is not a DDS file", nameof(imagepath));
                }

                height = BitConverter.ToUInt32(header, 12);
                width = BitConverter.ToUInt32(header, 16);
                var linearSize = BitConverter.ToUInt32(header, 20);
                mipMapCount = BitConverter.ToUInt32(header, 28);
                var fourCC = Encoding.ASCII.GetString(header, 84, 4);
                if (!DDSPixelFormats.TryGetValue(Encoding.ASCII.GetString(header, 84, 4), out format))
                {
                    throw new ArgumentException($"Specified file uses unsupported internal format {fourCC}", nameof(imagepath));
                }

                //uint components = (fourCC == FOURCC_DXT1) ? 3u : 4u;

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
		        if (width < 1) width = 1;
		        if (height < 1) height = 1;
	        } 

	        return textureId;
        }
    }
}
