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
    public class StaticColoredRenderer : IRenderer
    {
        private uint[] textures;

        private uint[] vertexArrayIds;
        private uint[] vertexBuffers;
        IList<Vector3> vertexPositions;
        IList<Vector3> vertexNormals;
        IList<Vector3> vertexColors;
        IList<uint> indices;

        private uint programId;
        string vertexShader;
        string fragmentShader;
        private Uniform mvpMatrix;
        private Uniform viewMatrix;
        private Uniform modelMatrix;
        private Uniform textureSampler;
        private Uniform lightPosition;

        ICamera camera;

        static DateTime lastUpdate = DateTime.Now;

        public StaticColoredRenderer(
            IList<Vector3> vertexPositions,
            IList<Vector3> vertexNormals,
            IList<Vector3> vertexColors,
            IList<uint> indices,
            ICamera camera,
            string vertexShader = StandardShaders.ColoredVertex,
            string fragmentShader = StandardShaders.ColoredFragment)
        {
            this.vertexPositions = vertexPositions;
            this.vertexNormals = vertexNormals;
            this.vertexColors = vertexColors;
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

            // Create and compile our GLSL program from the shaders
            programId = new ProgramBuilder()
                .WithStandardShader(ShaderType.VertexShader, vertexShader)
                .WithStandardShader(ShaderType.FragmentShader, fragmentShader)
                .Create();

            // Add our uniforms to our program
            mvpMatrix = new Uniform(programId, "MVP");
            viewMatrix = new Uniform(programId, "V");
            modelMatrix = new Uniform(programId, "M") { Value = Matrix4x4.Identity };
            textureSampler = new Uniform(programId, "myTextureSampler") { Value = 0 };
            lightPosition = new Uniform(programId, "LightPosition_worldspace") { Value = new Vector3(4, 4, 4) };

            // Generate and populate 4 vertex buffers:
            // One for positions, one for normals, one for texture coordinates, one for indices.
            vertexBuffers = new uint[4];
            Gl.GenBuffers(vertexBuffers);
            GlEx.BufferData(vertexBuffers[0], BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexPositions.ToArray());
            GlEx.BufferData(vertexBuffers[1], BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexColors.ToArray());
            GlEx.BufferData(vertexBuffers[2], BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexNormals.ToArray());
            GlEx.BufferData(vertexBuffers[3], BufferTarget.ElementArrayBuffer, BufferUsage.StaticDraw, indices.ToArray());
        }

        /// <inheritdoc />
        public void Render(object sender)
        {
            // Clear the screen and activate our program
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Gl.UseProgram(programId);

            // Compute the various transforms in accordance with the state of the camera
            mvpMatrix.Value = Matrix4x4.Transpose(camera.ViewMatrix * camera.ProjectionMatrix);
            viewMatrix.Value = Matrix4x4.Transpose(camera.ViewMatrix);

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
            Gl.DeleteProgram(programId);
            Gl.DeleteTextures(textures);
            Gl.DeleteVertexArrays(vertexArrayIds);
        }
    }
}
