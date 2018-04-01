namespace OpenGlHelpers.Core
{
    using OpenGL;
    using OpenGlHelpers.Core.LowLevel;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// Renderer class for static 3D geometry.
    /// </summary>
    public class ColoredStaticMesh : IRenderable
    {
        private const string ShaderResourceNamePrefix = "OpenGlHelpers.Core.Meshes";

        private readonly IViewProjection viewProjection;

        private ProgramBuilder programBuilder;
        private Program program;
        private VertexArrayObjectBuilder vertexArrayObjectBuilder;
        private VertexArrayObject vertexArrayObject;

        public ColoredStaticMesh(
            IViewProjection viewProjection,
            IList<Vector3> vertexPositions,
            IList<Vector3> vertexNormals,
            IList<Vector3> vertexColors,
            IList<uint> indices)
        {
            this.viewProjection = viewProjection;

            // TODO: allow program to be shared..
            this.programBuilder = new ProgramBuilder()
                .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Colored.Vertex.glsl")
                .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Colored.Fragment.glsl")
                .WithUniforms("MVP", "V", "M", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor");

            this.vertexArrayObjectBuilder = new VertexArrayObjectBuilder(PrimitiveType.Triangles)
                .WithBuffer(BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexPositions.ToArray())
                .WithBuffer(BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexColors.ToArray())
                .WithBuffer(BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexNormals.ToArray())
                .WithIndex(indices.ToArray());
        }

        /// <summary>
        /// Gets or sets the model transform for this mesh.
        /// </summary>
        public Matrix4x4 Model { get; set; } = Matrix4x4.Identity;

        /// <inheritdoc />
        public void ContextCreated(DeviceContext deviceContext)
        {
            this.program = this.programBuilder.Build();
            this.vertexArrayObject = this.vertexArrayObjectBuilder.Build();
        }

        /// <inheritdoc />
        public void Render(DeviceContext deviceContext)
        {
            this.program.UseWithUniformValues(
                Matrix4x4.Transpose(this.Model * this.viewProjection.View * this.viewProjection.Projection),
                Matrix4x4.Transpose(this.viewProjection.View),
                Matrix4x4.Transpose(this.Model),
                new Vector3(4, 4, 4),
                new Vector3(1, 1, 1),
                30f,
                new Vector3(0.3f, 0.3f, 0.3f));
            this.vertexArrayObject.Draw();
        }

        /// <inheritdoc />
        public void ContextDestroying(DeviceContext deviceContext)
        {
            this.vertexArrayObject.Dispose();
            this.program.Dispose();
        }
    }
}
