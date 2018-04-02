namespace OpenGlHelpers.Core
{
    using OpenGL;
    using OpenGlHelpers.Core.LowLevel;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// Renderable class for static 3D geometry.
    /// </summary>
    public class StaticTexuredRenderer : IRenderable
    {
        private const string ShaderResourceNamePrefix = "OpenGlHelpers.Core.Renderables.Basic";

        private readonly IViewProjection viewProjection;

        private uint[] textures;
        private GlProgramBuilder programBuilder;
        private GlProgram program;
        private GlVertexArrayObjectBuilder vertexArrayObjectBuilder;
        private GlVertexArrayObject vertexArrayObject;

        public StaticTexuredRenderer(
            IViewProjection viewProjection,
            IList<Vector3> vertexPositions,
            IList<Vector3> vertexNormals,
            IList<Vector2> vertexTextureCoordinates,
            IList<uint> indices)
        {
            this.viewProjection = viewProjection;

            // TODO: Allow program to be shared
            this.programBuilder = new GlProgramBuilder()
                .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Textured.Vertex.glsl")
                .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Textured.Fragment.glsl")
                .WithUniforms("MVP", "V", "M", "myTextureSampler", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor");

            this.vertexArrayObjectBuilder = new GlVertexArrayObjectBuilder(PrimitiveType.Triangles)
                .WithAttribute(BufferUsage.StaticDraw, vertexPositions.ToArray())
                .WithAttribute(BufferUsage.StaticDraw, vertexTextureCoordinates.ToArray())
                .WithAttribute(BufferUsage.StaticDraw, vertexNormals.ToArray())
                .WithIndex(indices.ToArray());
        }

        public Matrix4x4 Model { get; set; } = Matrix4x4.Identity;

        /// <inheritdoc />
        public void ContextCreated(DeviceContext deviceContext)
        {
            this.textures = new uint[1];
            this.textures[0] = TextureLoader.LoadDDS("Assets/uvmap.DDS");

            this.program = this.programBuilder.Build();
            this.programBuilder = null;
            this.vertexArrayObject = this.vertexArrayObjectBuilder.Build();
            this.vertexArrayObjectBuilder = null;
        }

        /// <inheritdoc />
        public void Render(DeviceContext deviceContext)
        {
            program.UseWithUniformValues(
                Matrix4x4.Transpose(this.Model * this.viewProjection.View * this.viewProjection.Projection),
                Matrix4x4.Transpose(this.viewProjection.View),
                Matrix4x4.Transpose(this.Model),
                0,
                new Vector3(4, 4, 4),
                new Vector3(1, 1, 1),
                30f,
                new Vector3(0.3f, 0.3f, 0.3f));

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2d, textures[0]);

            this.vertexArrayObject.Draw();
        }

        /// <inheritdoc />
        public void ContextDestroying(DeviceContext deviceContext)
        {
            this.vertexArrayObject.Dispose();
            this.program.Dispose();
            Gl.DeleteTextures(textures);
        }
    }
}
