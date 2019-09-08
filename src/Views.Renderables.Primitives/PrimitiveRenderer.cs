namespace GLHDN.Views.Renderables.Primitives
{
    using GLHDN.Core;
    using GLHDN.Core.VaoDecorators;
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
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.Primitives";

        private static readonly object stateLock = new object();
        private static GlProgramBuilder programBuilder;
        private static GlProgram program;

        private readonly IViewProjection camera;
        private readonly IObservable<IObservable<IList<Primitive>>> source;
        private readonly int capacity;

        private GlVertexArrayObjectBuilder coloredTriangleBufferBuilder;
        private ReactiveBuffer<PrimitiveVertex> coloredTriangleBuffer;
        private GlVertexArrayObjectBuilder coloredLineBufferBuilder;
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
            this.capacity = capacity;

            if (program == null && programBuilder == null)
            {
                lock (stateLock)
                {
                    if (program == null && programBuilder == null)
                    {
                        programBuilder = new GlProgramBuilder()
                            .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Colored.Vertex.glsl")
                            .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Colored.Fragment.glsl")
                            .WithUniforms("MVP", "V", "M", "AmbientLightColor", "DirectedLightDirection", "DirectedLightColor", "LightPosition_worldspace", "LightColor", "LightPower");
                    }
                }
            }

            this.coloredTriangleBufferBuilder = new GlVertexArrayObjectBuilder(PrimitiveType.Triangles)
                .ForReactiveBuffer<PrimitiveVertex>(capacity, new[] { 0, 1, 2 });

            this.coloredLineBufferBuilder = new GlVertexArrayObjectBuilder(PrimitiveType.Lines)
                .ForReactiveBuffer<PrimitiveVertex>(capacity, new[] { 0, 1 });
        }

        /// <summary>
        /// Gets the lighting applied as a minimum to every fragment.
        /// </summary>
        public Color AmbientLightColor { get; set; } = Color.Transparent;

        /// <summary>
        /// Gets the directed light direction. Fragments facing this direction are lit with the directed light color.
        /// </summary>
        public Vector3 DirectedLightDirection { get; set; } = Vector3.Zero;

        /// <summary>
        /// Gets the directed light color, applied to fragments facing the directed light direction.
        /// </summary>
        public Color DirectedLightColor { get; set; } = Color.Transparent;

        /// <summary>
        /// Gets the point light position. Fragments facing this position are lit with the point light color.
        /// </summary>
        public Vector3 PointLightPosition { get; set; } = Vector3.Zero;

        /// <summary>
        /// Gets the point light color, applied to fragments facing the point light position.
        /// </summary>
        public Color PointLightColor { get; set; } = Color.Transparent;

        /// <summary>
        /// Gets the point light power. Affects the intensity with which fragments are lit by the point light.
        /// </summary>
        public float PointLightPower { get; set; } = 0f;

        /// <inheritdoc />
        public void Load()
        {
            ThrowIfDisposed();

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

            this.coloredTriangleBuffer = new ReactiveBuffer<PrimitiveVertex>(
                this.source.Select(pso => 
                    pso.Select(ps => 
                        ps.Where(p => p.IsTrianglePrimitive).SelectMany(p => p.Vertices).ToList())),
                new[] { 0, 1, 2 },
                new SynchronizedVao(this.coloredTriangleBufferBuilder.Build()));

            this.coloredLineBuffer = new ReactiveBuffer<PrimitiveVertex>(
                this.source.Select(pso =>
                    pso.Select(ps =>
                        ps.Where(p => !p.IsTrianglePrimitive).SelectMany(p => p.Vertices).ToList())),
                new[] { 0, 1, },
                new SynchronizedVao(this.coloredLineBufferBuilder.Build()));
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
