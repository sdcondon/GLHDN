namespace GLHDN.Views.Renderables.BasicExamples
{
    using GLHDN.Core;
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// Simple renderable class for static 3D geometry.
    /// </summary>
    public class ColoredStaticMesh : IRenderable
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.BasicExamples";

        private static readonly object ProgramStateLock = new object();
        private static ProgramBuilder programBuilder;
        private static GlProgram program;

        private readonly IViewProjection viewProjection;

        private VertexArrayObjectBuilder vertexArrayObjectBuilder;
        private GlVertexArrayObject vertexArrayObject;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredStaticMesh"/> class.
        /// </summary>
        /// <param name="viewProjection">The provider for the view and projection matrices to use when rendering.</param>
        /// <param name="vertexPositions">The positions of the vertices of the mesh.</param>
        /// <param name="vertexNormals">The normals of the vertices of the mesh.</param>
        /// <param name="vertexColors">The color of the vertices of the mesh.</param>
        /// <param name="indices">The indices (into the provided vertices) to use for actually rendering the mesh.</param>
        public ColoredStaticMesh(
            IViewProjection viewProjection,
            IList<Vector3> vertexPositions,
            IList<Vector3> vertexNormals,
            IList<Vector3> vertexColors,
            IList<uint> indices)
        {
            this.viewProjection = viewProjection;

            if (program == null && programBuilder == null)
            {
                lock (ProgramStateLock)
                {
                    if (program == null && programBuilder == null)
                    {
                        programBuilder = new ProgramBuilder()
                            .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Colored.Vertex.glsl")
                            .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Colored.Fragment.glsl")
                            .WithUniforms("MVP", "V", "M", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor");
                    }
                }
            }

            this.vertexArrayObjectBuilder = new VertexArrayObjectBuilder(PrimitiveType.Triangles)
                .WithAttributeBuffer(BufferUsage.StaticDraw, vertexPositions.ToArray())
                .WithAttributeBuffer(BufferUsage.StaticDraw, vertexColors.ToArray())
                .WithAttributeBuffer(BufferUsage.StaticDraw, vertexNormals.ToArray())
                .WithIndex(indices.ToArray());
        }

        /// <summary>
        /// Gets or sets the model transform for this mesh.
        /// </summary>
        public Matrix4x4 Model { get; set; } = Matrix4x4.Identity;

        /// <inheritdoc />
        public void Load()
        {
            ThrowIfDisposed();

            if (program == null)
            {
                lock (ProgramStateLock)
                {
                    if (program == null)
                    {
                        program = programBuilder.Build();
                        programBuilder = null;
                    }
                }
            }

            this.vertexArrayObject = (GlVertexArrayObject)this.vertexArrayObjectBuilder.Build();
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
                new Vector3(4, 4, 4),
                new Vector3(1, 1, 1),
                30f,
                new Vector3(0.3f, 0.3f, 0.3f));
            this.vertexArrayObject.Draw(-1);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.vertexArrayObject?.Dispose();
            isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
