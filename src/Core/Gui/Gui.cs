namespace OpenGlHelpers.Core
{
    using System.Collections.Generic;
    using System.Numerics;
    using OpenGL;
    using OpenGlHelpers.Core.LowLevel;

    /// <summary>
    /// Renderer for graphical user interface elements.
    /// </summary>
    class Gui : IRenderable
    {
        private const string ShaderResourceNamePrefix = "OpenGlHelpers.Core.Gui";

        private ProgramBuilder programBuilder;
        private Program program;
        private VertexArrayObjectBuilder vertexArrayObjectBuilder;
        private VertexArrayObject vertexArrayObject;

        public Gui()
        {
            // TODO: allow program to be shared..
            this.programBuilder = new ProgramBuilder()
                .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Gui.Vertex.glsl")
                .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Gui.Fragment.glsl")
                .WithUniforms("MVP", "V", "M");

            this.vertexArrayObjectBuilder = new VertexArrayObjectBuilder(PrimitiveType.Triangles)
                .WithBuffer(BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexPositions.ToArray())
                .WithBuffer(BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexColors.ToArray());
        }

        /// <summary>
        /// Gets the list of elements being rendered.
        /// </summary>
        public IList<GuiElement> Elements
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public void ContextCreated(DeviceContext deviceContext)
        {
            this.program = this.programBuilder.Build();
            this.programBuilder = null;
            this.vertexArrayObject = this.vertexArrayObjectBuilder.Build();
            this.vertexArrayObjectBuilder = null;
        }

        /// <inheritdoc />
        public void Render(DeviceContext deviceContext, Matrix4x4 model, Matrix4x4 view, Matrix4x4 projection)
        {
            // TODO: Apply updates (or look into streaming?)

            // Assume GUI goes on top of everything drawn already - so clear the depth buffer
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            // TODO: HARD-CODE IN SHADER INSTEAD?
            projection = Matrix4x4.CreateOrthographic(width, height, 0f, 1f);
            view = Matrix4x4.CreateLookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);
            model = Matrix4x4.Identity;

            this.program.UseWithUniformValues(
                Matrix4x4.Transpose(model * view * projection),
                Matrix4x4.Transpose(view),
                Matrix4x4.Transpose(model));
            this.vertexArrayObject.Draw();
        }

        /// <inheritdoc />
        public void ContextDestroying(DeviceContext deviceContext)
        {
            this.program.Dispose();
            this.vertexArrayObject.Dispose();
        }

        private void ElementsToBuffer()
        {

        }
    }
}
