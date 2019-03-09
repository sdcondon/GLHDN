namespace GLHDN.Views.Renderables.BasicExamples
{
    using GLHDN.Core;
    using OpenGL;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// Renderable class for static 3D geometry.
    /// </summary>
    public class StaticTexuredRenderer : IRenderable
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.BasicExamples";

        private readonly IViewProjection viewProjection;
        private readonly string textureFilePath;

        private uint[] textures;
        private GlProgramBuilder programBuilder;
        private GlProgram program;
        private GlVertexArrayObjectBuilder vertexArrayObjectBuilder;
        private GlVertexArrayObject vertexArrayObject;

        public StaticTexuredRenderer(
            IViewProjection viewProjection,
            IEnumerable<Vertex> vertices,
            IEnumerable<uint> indices,
            string textureFilePath)
        {
            this.viewProjection = viewProjection;

            // TODO: Allow program to be shared
            this.programBuilder = new GlProgramBuilder()
                .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Textured.Vertex.glsl")
                .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Textured.Fragment.glsl")
                .WithUniforms("MVP", "V", "M", "myTextureSampler", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor");

            this.vertexArrayObjectBuilder = new GlVertexArrayObjectBuilder(PrimitiveType.Triangles)
                .WithAttributeBuffer(BufferUsage.StaticDraw, vertices.ToArray())
                .WithIndex(indices.ToArray());

            this.textureFilePath = textureFilePath;
        }

        public Matrix4x4 Model { get; set; } = Matrix4x4.Identity;

        /// <inheritdoc />
        public void ContextCreated(DeviceContext deviceContext)
        {
            this.textures = new uint[1];
            this.textures[0] = TextureLoader.LoadDDS(textureFilePath);

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

        public struct Vertex
        {
            public Vector3 P;
            public Vector2 UV;
            public Vector3 N;

            public Vertex(Vector3 position, Vector2 uv, Vector3 normal)
            {
                P = position;
                UV = uv;
                N = normal;
            }
        }
    }
}
