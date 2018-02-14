namespace OpenGlHelpers.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// Renderer class for static geometry.
    /// </summary>
    public class StaticTexuredRenderer : IRenderer
    {
        private uint[] textures;

        private uint[] vertexArrayIds;
        private uint[] vertexBuffers;
        IList<Vector3> vertexPositions;
        IList<Vector3> vertexNormals;
        IList<Vector2> vertexTextureCoordinates;
        IList<uint> indices;

        private Program program;
        string vertexShader;
        string fragmentShader;

        ICamera camera;

        static DateTime lastUpdate = DateTime.Now;

        public StaticTexuredRenderer(
            IList<Vector3> vertexPositions,
            IList<Vector3> vertexNormals,
            IList<Vector2> vertexTextureCoordinates,
            IList<uint> indices,
            ICamera camera,
            string vertexShader = StandardShaders.TexturedVertex,
            string fragmentShader = StandardShaders.TexturedFragment)
        {
            this.vertexPositions = vertexPositions;
            this.vertexNormals = vertexNormals;
            this.vertexTextureCoordinates = vertexTextureCoordinates;
            this.indices = indices;
            this.camera = camera;
            this.vertexShader = vertexShader;
            this.fragmentShader = fragmentShader;
        }

        /// <inheritdoc />
        public void ContextCreated(object sender)
        {
            // 
            Gl.ClearColor(0.0f, 0.0f, 0.1f, 0.0f); // Dark blue background    
            Gl.Enable(EnableCap.DepthTest); // Enable depth test
            Gl.DepthFunc(DepthFunction.Less); // Accept fragment if it closer to the camera than the former one
            Gl.Enable(EnableCap.CullFace); // Cull triangles which normal is not towards the camera

            //
            vertexArrayIds = new uint[1];
            Gl.GenVertexArrays(vertexArrayIds);
            Gl.BindVertexArray(vertexArrayIds[0]);

            // Load the texture
            textures = new uint[1];
            textures[0] = TextureLoader.LoadDDS("Assets/uvmap.DDS");

            // Create and compile our GLSL program from the shaders
            program = new ProgramBuilder()
                .WithStandardShader(ShaderType.VertexShader, vertexShader)
                .WithStandardShader(ShaderType.FragmentShader, fragmentShader)
                .WithUniforms("MVP", "V", "M", "myTextureSampler", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor")
                .Create();

            // Generate and populate 4 vertex buffers:
            // One for positions, one for normals, one for texture coordinates, one for indices.
            vertexBuffers = new uint[4];
            Gl.GenBuffers(vertexBuffers);
            GlEx.BufferData(vertexBuffers[0], BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexPositions.ToArray());
            GlEx.BufferData(vertexBuffers[1], BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexTextureCoordinates.ToArray());
            GlEx.BufferData(vertexBuffers[2], BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexNormals.ToArray());
            GlEx.BufferData(vertexBuffers[3], BufferTarget.ElementArrayBuffer, BufferUsage.StaticDraw, indices.ToArray());
        }

        /// <inheritdoc />
        public void Render(object sender)
        {
            // Clear the buffers
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Activate our program and
            // compute the various transforms in accordance with the state of the camera
            Gl.UseProgram(program.Id);
            program.SetUniformValues(
                Matrix4x4.Transpose(camera.ViewMatrix * camera.ProjectionMatrix),
                Matrix4x4.Transpose(camera.ViewMatrix),
                Matrix4x4.Identity,
                0,
                new Vector3(4, 4, 4),
                new Vector3(1, 1, 1),
                30f,
                new Vector3(0.3f, 0.3f, 0.3f));

            // Bind our texture in Texture Unit 0
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2d, textures[0]);

            // 1st attribute buffer : vertices
            Gl.EnableVertexAttribArray(0);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffers[0]);
            Gl.VertexAttribPointer(
                0,                  // attribute. No particular reason for 0, but must match the layout in the shader.
                3,                  // size
                VertexAttribType.Float, // type
                false,              // normalized?
                0,                  // stride
                IntPtr.Zero         // array buffer offset
            );

            // 2nd attribute buffer : UVs
            Gl.EnableVertexAttribArray(1);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffers[1]);
            Gl.VertexAttribPointer(
                1,                                // attribute. No particular reason for 1, but must match the layout in the shader.
                2,                                // size : U+V => 2
                VertexAttribType.Float,           // type
                false,                            // normalized?
                0,                                // stride
                IntPtr.Zero                       // array buffer offset
            );

            // 3rd attribute buffer : normals
            Gl.EnableVertexAttribArray(2);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffers[2]);
            Gl.VertexAttribPointer(
                2,                      // attribute
                3,                      // size
                VertexAttribType.Float, // type
                false,                  // normalized?
                0,                      // stride
                IntPtr.Zero             // array buffer offset
            );

            // Bind index buffer & draw
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, vertexBuffers[3]);
            Gl.DrawElements(
                PrimitiveType.Triangles,        // mode
                indices.Count,                  // count
                DrawElementsType.UnsignedInt,   // type
                IntPtr.Zero                     // element array buffer offset
            );

            Gl.DisableVertexAttribArray(0);
            Gl.DisableVertexAttribArray(1);
            Gl.DisableVertexAttribArray(2);
        }

        /// <inheritdoc />
        public void ContextUpdate(object sender)
        {
        }

        /// <inheritdoc />
        public void ContextDestroying(object sender)
        {
            Gl.DeleteBuffers(vertexBuffers);
            Gl.DeleteProgram(program.Id);
            Gl.DeleteTextures(textures);
            Gl.DeleteVertexArrays(vertexArrayIds);
        }
    }
}
