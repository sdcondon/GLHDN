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

        private static readonly object stateLock = new object();
        private static GlProgramBuilder programBuilder;
        private static GlProgram program;

        private readonly IViewProjection viewProjection;
        private readonly string textureFilePath;

        private uint[] textures;
        private GlVertexArrayObjectBuilder vertexArrayObjectBuilder;
        private GlVertexArrayObject vertexArrayObject;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturedStaticMesh"/> class.
        /// </summary>
        /// <param name="viewProjection">Provider for view and projection matrices.</param>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="textureFilePath"></param>
        public TexturedStaticMesh(
            IViewProjection viewProjection,
            IEnumerable<Vertex> vertices,
            IEnumerable<uint> indices,
            string textureFilePath)
        {
            this.viewProjection = viewProjection;

            if (program == null && programBuilder == null)
            {
                lock (stateLock)
                {
                    if (program == null && programBuilder == null)
                    {
                        programBuilder = new GlProgramBuilder()
                            .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Textured.Vertex.glsl")
                            .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Textured.Fragment.glsl")
                            .WithUniforms("MVP", "V", "M", "myTextureSampler", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor");
                    }
                }
            }

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

            if (program == null)
            {
                lock (stateLock)
                {
                    if (program == null)
                    {
                        program = programBuilder.Build();
                        programBuilder = null;
                    }
                }
            }

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

        /// <summary>
        /// Container struct for the attributes of a vertex.
        /// </summary>
        public struct Vertex
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Vertex"/> struct.
            /// </summary>
            /// <param name="position">The position of the vertex.</param>
            /// <param name="uv">The texture coordinate of the vertex.</param>
            /// <param name="normal">The normal vector of the vertex.</param>
            public Vertex(Vector3 position, Vector2 uv, Vector3 normal)
            {
                Position = position;
                UV = uv;
                Normal = normal;
            }

            /// <summary>
            /// Gets the position of the vertex.
            /// </summary>
            public Vector3 Position { get; }

            /// <summary>
            /// Gets the texture coordinate of the vertex.
            /// </summary>
            public Vector2 UV { get; }

            /// <summary>
            /// Gets the normal vector of the vertex.
            /// </summary>
            public Vector3 Normal { get; }
        }
    }
}
