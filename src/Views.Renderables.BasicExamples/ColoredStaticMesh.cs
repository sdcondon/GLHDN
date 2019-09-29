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
        /// <param name="vertices">The vertices of the mesh.</param>
        /// <param name="indices">The indices (into the provided vertices) to use for actually rendering the mesh.</param>
        public ColoredStaticMesh(
            IViewProjection viewProjection,
            IEnumerable<Vertex> vertices,
            IEnumerable<uint> indices)
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
                .WithAttributeBuffer(BufferUsage.StaticDraw, vertices.ToArray())
                .WithIndex(indices.ToArray());
        }

        /// <summary>
        /// Gets or sets the model transform for this mesh.
        /// </summary>
        public Matrix4x4 Model { get; set; } = Matrix4x4.Identity;

        /// <summary>
        /// Gets or sets the lighting applied as a minimum to every fragment.
        /// </summary>
        public Color AmbientLightColor { get; set; } = Color.Transparent();

        /// <summary>
        /// Gets or sets the point light position. Fragments facing this position are lit with the point light color.
        /// </summary>
        public Vector3 PointLightPosition { get; set; } = Vector3.Zero;

        /// <summary>
        /// Gets or sets the point light color, applied to fragments facing the point light position.
        /// </summary>
        public Color PointLightColor { get; set; } = Color.Transparent();

        /// <summary>
        /// Gets or sets the point light power. Affects the intensity with which fragments are lit by the point light.
        /// </summary>
        public float PointLightPower { get; set; } = 0f;

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
                PointLightPosition,
                (Vector3)PointLightColor,
                PointLightPower,
                (Vector3)AmbientLightColor);
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

        /// <summary>
        /// Container struct for the attributes of a vertex.
        /// </summary>
        public struct Vertex
        {
            /// <summary>
            /// Gets the position of the vertex.
            /// </summary>
            public readonly Vector3 Position;

            /// <summary>
            /// Gets the color of the vertex.
            /// </summary>
            public readonly Vector3 Color;

            /// <summary>
            /// Gets the normal vector of the vertex.
            /// </summary>
            public readonly Vector3 Normal;

            /// <summary>
            /// Initializes a new instance of the <see cref="Vertex"/> struct.
            /// </summary>
            /// <param name="position">The position of the vertex.</param>
            /// <param name="color">The color of the vertex.</param>
            /// <param name="normal">The normal vector of the vertex.</param>
            public Vertex(Vector3 position, Vector3 color, Vector3 normal)
            {
                Position = position;
                Color = color;
                Normal = normal;
            }
        }
    }
}
