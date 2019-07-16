namespace GLHDN.Views.Renderables.BasicExamples
{
    using GLHDN.Core;
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// Renderable class for static 3D geometry.
    /// </summary>
    public class TexturedStaticMesh : IRenderable
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.BasicExamples";

        private readonly IViewProjection viewProjection;
        private readonly string textureFilePath;

        private uint[] textures;
        private GlProgramBuilder programBuilder;
        private GlProgram program;
        private GlVertexArrayObjectBuilder vertexArrayObjectBuilder;
        private GlVertexArrayObject vertexArrayObject;
        private bool isDisposed;

        public TexturedStaticMesh(
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

        /// <summary>
        /// Finalizes an instance of the <see cref="TexturedStaticMesh"/> class.
        /// </summary>
        ~TexturedStaticMesh()
        {
            Gl.DeleteTextures(textures);
        }

        /// <summary>
        /// Gets or sets the model transform for this mesh.
        /// </summary>
        public Matrix4x4 Model { get; set; } = Matrix4x4.Identity;

        /// <inheritdoc />
        public void Load()
        {
            ThrowIfDisposed();

            this.textures = new uint[1];
            this.textures[0] = Path.GetExtension(textureFilePath) == ".DDS" ? TextureLoader.LoadDDS(textureFilePath) : TextureLoader.LoadBMP(textureFilePath);

            this.program = this.programBuilder.Build();
            this.programBuilder = null;
            this.vertexArrayObject = this.vertexArrayObjectBuilder.Build();
            this.vertexArrayObjectBuilder = null;
        }

        /// <inheritdoc />
        public void Update(TimeSpan elapsed)
        {
        }

        /// <inheritdoc />
        public void Render()
        {
            ThrowIfDisposed();

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
        public void Dispose()
        {
            this.vertexArrayObject.Dispose();
            this.program.Dispose();
            Gl.DeleteTextures(textures);
            GC.SuppressFinalize(this);
            isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public struct Vertex
        {
            public readonly Vector3 Position;
            public readonly Vector2 UV;
            public readonly Vector3 Normal;

            public Vertex(Vector3 position, Vector2 uv, Vector3 normal)
            {
                Position = position;
                UV = uv;
                Normal = normal;
            }
        }
    }
}
