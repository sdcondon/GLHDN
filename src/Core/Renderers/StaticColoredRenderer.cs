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

        IList<Vector3> vertexPositions;
        IList<Vector3> vertexNormals;
        IList<Vector3> vertexColors;
        IList<uint> indices;
        private VertexArrayObject vertexArrayObject;

        private Program program;
        string vertexShader;
        string fragmentShader;

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

            // Create and compile our GLSL program from the shaders
            program = new ProgramBuilder()
                .WithStandardShader(ShaderType.VertexShader, vertexShader)
                .WithStandardShader(ShaderType.FragmentShader, fragmentShader)
                .WithUniforms("MVP", "V", "M", "myTextureSampler", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor")
                .Create();

            // Generate and populate 4 vertex buffers:
            // One for positions, one for normals, one for texture coordinates, one for indices.
            this.vertexArrayObject = new VertexArrayObjectBuilder()
                .WithBuffer(BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexPositions.ToArray())
                .WithBuffer(BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexColors.ToArray())
                .WithBuffer(BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexNormals.ToArray())
                .WithIndex(indices.ToArray())
                .Create(PrimitiveType.Triangles);
        }

        /// <inheritdoc />
        public void Render(object sender)
        {
            // Clear the buffers
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Aactivate our program and compute the various transforms in accordance with the state of the camera
            program.UseWithUniformValues(
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

            // Draw the vertexArrayObject using the active program
            this.vertexArrayObject.Draw();
        }

        /// <inheritdoc />
        public void ContextUpdate(object sender)
        {
        }

        /// <inheritdoc />
        public void ContextDestroying(object sender)
        {
            vertexArrayObject.Dispose();
            program.Dispose();
            Gl.DeleteTextures(textures);
        }
    }
}
