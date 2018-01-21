namespace OpenGlHelpers.Core
{
    using OpenGL;
    using System;
    using System.IO;
    using System.Text;

    public static class TextureLoader
    {
        private const uint FOURCC_DXT1 = 0x31545844; // Equivalent to "DXT1" in ASCII
        private const uint FOURCC_DXT3 = 0x33545844; // Equivalent to "DXT3" in ASCII
        private const uint FOURCC_DXT5 = 0x35545844; // Equivalent to "DXT5" in ASCII

        public static uint LoadDDS(string imagepath)
        {
            uint height;
            uint width;
            uint linearSize;
            uint mipMapCount;
            uint fourCC;
            byte[] buffer;

            using (var file = File.Open(imagepath, FileMode.Open))
            {
                /* verify the type of file */
                var filecode = new byte[4];

                file.Read(filecode, 0, 4);
                if (Encoding.ASCII.GetString(filecode) != "DDS ")
                {
                    return 0;
                }

                /* get the surface desc */
                var header = new byte[124];
                file.Read(header, 0, 124);

                height = BitConverter.ToUInt32(header, 8);//*(unsigned int*)&(header[8]);
                width = BitConverter.ToUInt32(header, 12);//*(unsigned int*)&(header[12]);
                linearSize = BitConverter.ToUInt32(header, 16);//*(unsigned int*)&(header[16]);
                mipMapCount = BitConverter.ToUInt32(header, 24);//*(unsigned int*)&(header[24]);
                fourCC = BitConverter.ToUInt32(header, 80);//*(unsigned int*)&(header[80]);

                /* how big is it going to be including all mipmaps? */
                var bufsize = (int)(mipMapCount > 1 ? linearSize * 2 : linearSize);
                buffer = new byte[bufsize];

                file.Read(buffer, 0, bufsize);
            }

            uint components = (fourCC == FOURCC_DXT1) ? 3u : 4u;
            InternalFormat format;

	        switch(fourCC) 
	        { 
	            case FOURCC_DXT1:
                    format = InternalFormat.CompressedRgbaS3tcDxt1Ext;
		            break; 
	            case FOURCC_DXT3: 
		            format = InternalFormat.CompressedRgbaS3tcDxt3Ext;
                    break; 
	            case FOURCC_DXT5: 
		            format = InternalFormat.CompressedRgbaS3tcDxt5Ext;
                    break; 
	            default:
		            return 0; 
	        }

	        // Create one OpenGL texture
	        uint[] textureIds = new uint[1];
            Gl.GenTextures(textureIds);

            // "Bind" the newly created texture : all future texture functions will modify this texture
            Gl.BindTexture(TextureTarget.Texture2d, textureIds[0]);

            Gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            uint blockSize = (format == InternalFormat.CompressedRgbaS3tcDxt1Ext) ? 8u : 16u;
            uint offset = 0;

	        /* load the mipmaps */ 
	        for (int level = 0; level < mipMapCount && (width > 0 || height > 0); ++level) 
	        { 
		        uint size = ((width + 3) / 4) * ((height + 3) / 4) * blockSize;

                var buf = new byte[size];
                Array.Copy(buffer, offset, buf, 0, size);
                Gl.CompressedTexImage2D(TextureTarget.Texture2d, level, format, (int)width, (int)height, 0, (int)size, buf);

                offset += size; 
		        width /= 2; 
		        height /= 2; 

		        // Deal with Non-Power-Of-Two textures. This code is not included in the webpage to reduce clutter.
		        if (width < 1) width = 1;
		        if (height < 1) height = 1;
	        } 

	        return textureIds[0];
        }
    }
}
