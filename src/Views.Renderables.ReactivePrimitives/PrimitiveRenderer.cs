namespace GLHDN.Views.Renderables.ReactivePrimitives
{
    using GLHDN.Core;
    using GLHDN.ReactiveBuffers;
    using GLHDN.Views;
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reactive.Linq;

    /// <summary>
    /// Implementation of <see cref="IRenderable" /> that renders a set of primitive shapes from an observable sequence of source data.
    /// </summary>
    public class PrimitiveRenderer : IRenderable
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.ReactivePrimitives";

        private static readonly object ProgramStateLock = new object();
        private static ProgramBuilder programBuilder;
        private static GlProgram program;

        private readonly IViewProjection camera;
        private readonly IObservable<IObservable<IList<Primitive>>> source;

        private ReactiveBufferBuilder<PrimitiveVertex> coloredTriangleBufferBuilder;
        private ReactiveBuffer<PrimitiveVertex> coloredTriangleBuffer;
        private ReactiveBufferBuilder<PrimitiveVertex> coloredLineBufferBuilder;
        private ReactiveBuffer<PrimitiveVertex> coloredLineBuffer;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveRenderer"/> class.
        /// </summary>
        /// <param name="camera">Provider for view and projection matrices.</param>
        /// <param name="source">Source data. Outer sequence pushes different renderable entities, each of which pushes each time its state changes.</param>
        /// <param name="capacity">The maximum number of triangles and lines that can be rendered at once.</param>
        public PrimitiveRenderer(
            IViewProjection camera,
            IObservable<IObservable<IList<Primitive>>> source,
            int capacity)
        {
            this.camera = camera;
            this.source = source;

            if (program == null && programBuilder == null)
            {
                lock (ProgramStateLock)
                {
                    if (program == null && programBuilder == null)
                    {
                        programBuilder = new ProgramBuilder()
                            .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Colored.Vertex.glsl")
                            .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Colored.Fragment.glsl")
                            .WithUniforms("MVP", "V", "M", "AmbientLightColor", "DirectedLightDirection", "DirectedLightColor", "LightPosition_worldspace", "LightColor", "LightPower");
                    }
                }
            }

            // Re-use a single vertex list for every primitive to reduce GC burden - NB not re-entrant
            var triangleVertexList = new List<PrimitiveVertex>();
            this.coloredTriangleBufferBuilder = new ReactiveBufferBuilder<PrimitiveVertex>(
                PrimitiveType.Triangles,
                capacity,
                new[] { 0, 1, 2 },
                this.source.Select(pso => pso.Select(ps =>
                {
                    triangleVertexList.Clear();
                    for (int i = 0; i < ps.Count; i++)
                    {
                        if (ps[i].IsTrianglePrimitive)
                        {
                            triangleVertexList.AddRange(ps[i].Vertices);
                        }
                    }

                    return triangleVertexList;
                })));

            // Re-use a single vertex list for every primitive to reduce GC burden - NB not re-entrant
            var lineVertexList = new List<PrimitiveVertex>();
            this.coloredLineBufferBuilder = new ReactiveBufferBuilder<PrimitiveVertex>(
                PrimitiveType.Lines,
                capacity,
                new[] { 0, 1 },
                this.source.Select(pso => pso.Select(ps =>
                {
                    lineVertexList.Clear();
                    for (int i = 0; i < ps.Count; i++)
                    {
                        if (!ps[i].IsTrianglePrimitive)
                        {
                            lineVertexList.AddRange(ps[i].Vertices);
                        }
                    }

                    return lineVertexList;
                })));
        }

        /// <summary>
        /// Gets or sets the lighting applied as a minimum to every fragment.
        /// </summary>
        public Color AmbientLightColor { get; set; } = Color.Transparent();

        /// <summary>
        /// Gets or sets the directed light direction. Fragments facing this direction are lit with the directed light color.
        /// </summary>
        public Vector3 DirectedLightDirection { get; set; } = Vector3.Zero;

        /// <summary>
        /// Gets or sets the directed light color, applied to fragments facing the directed light direction.
        /// </summary>
        public Color DirectedLightColor { get; set; } = Color.Transparent();

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

            this.coloredTriangleBuffer = coloredTriangleBufferBuilder.Build();
            this.coloredTriangleBufferBuilder = null;

            this.coloredLineBuffer = coloredLineBufferBuilder.Build();
            this.coloredLineBufferBuilder = null;
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
                Matrix4x4.Transpose(this.camera.View * this.camera.Projection),
                Matrix4x4.Transpose(this.camera.View),
                Matrix4x4.Transpose(Matrix4x4.Identity),
                (Vector3)AmbientLightColor,
                DirectedLightDirection,
                (Vector3)DirectedLightColor,
                PointLightPosition,
                (Vector3)PointLightColor,
                PointLightPower);
            this.coloredTriangleBuffer.Draw();
            this.coloredLineBuffer.Draw();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            coloredTriangleBuffer.Dispose();
            coloredLineBuffer.Dispose();
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
