namespace GLHDN.Views
{
    using OpenGL;
    using GLHDN.Core;
    using System;
    using System.Numerics;

    /// <summary>
    /// Renderable class for 3D lines. For debug utilities.
    /// </summary>
    public class ColoredLines : IRenderable
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.BasicRenderables";

        private readonly IViewProjection viewProjection;

        private GlProgramBuilder programBuilder;
        private GlProgram program;
        private ObjectBufferBuilder<Tuple<Vector3, Vector3>> objectBufferBuilder;
        private ObjectBuffer<Tuple<Vector3, Vector3>> objectBuffer;

        public ColoredLines(IViewProjection viewProjection)
        {
            this.viewProjection = viewProjection;

            // TODO: allow program to be shared..
            this.programBuilder = new GlProgramBuilder()
                .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Colored.Vertex.glsl")
                .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Colored.Fragment.glsl")
                .WithUniforms("MVP", "V", "M", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor");

            this.objectBufferBuilder = new ObjectBufferBuilder<Tuple<Vector3, Vector3>>(PrimitiveType.Lines, 2, 100)
                .WithAttribute(a => new[] { a.Item1, a.Item2 })
                .WithAttribute(a => new[] { Vector3.One, Vector3.One })
                .WithAttribute(a => new[] { a.Item1, a.Item2 })
                .WithIndices(new[] { 0, 1 }); // TODO: Change so not needed
        }

        public void AddLine(Vector3 start, Vector3 end)
        {
            this.objectBuffer.Add(Tuple.Create(start, end));
        }

        public void ClearLines()
        {
            this.objectBuffer.Clear();
        }

        /// <inheritdoc />
        public void ContextCreated(DeviceContext deviceContext)
        {
            this.program = this.programBuilder.Build();
            this.programBuilder = null;
            this.objectBuffer = this.objectBufferBuilder.Build();
            this.objectBufferBuilder = null;
        }

        /// <inheritdoc />
        public void Render(DeviceContext deviceContext)
        {
            this.program.UseWithUniformValues(
                Matrix4x4.Transpose(this.viewProjection.View * this.viewProjection.Projection),
                Matrix4x4.Transpose(this.viewProjection.View),
                Matrix4x4.Identity,
                new Vector3(4, 4, 4),
                new Vector3(1, 1, 1),
                30f,
                new Vector3(0.3f, 0.3f, 0.3f));
            this.objectBuffer.Draw();
        }

        /// <inheritdoc />
        public void ContextDestroying(DeviceContext deviceContext)
        {
            this.objectBuffer.Dispose();
            this.program.Dispose();
        }
    }
}
