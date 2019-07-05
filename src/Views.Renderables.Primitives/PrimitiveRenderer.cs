namespace GLHDN.Views.Renderables.Primitives
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
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.Primitives";

        private static object stateLock = new object();
        private static GlProgramBuilder programBuilder;
        private static GlProgram program;

        private readonly IViewProjection camera;
        private readonly IObservable<IObservable<IList<Primitive>>> source;
        private readonly int capacity;

        private ReactiveBuffer<PrimitiveVertex> coloredTriangleBuffer;
        private ReactiveBuffer<PrimitiveVertex> coloredLineBuffer;
        private bool isDisposed;

        /// <summary>
        /// The ambient light color, applied as a minimum to every fragment.
        /// </summary>
        public Vector3 AmbientLightColor { get; set; } = Vector3.Zero;

        public Vector3 DirectedLightDirection { get; set; } = Vector3.Zero;

        public Vector3 DirectedLightColor { get; set; } = Vector3.Zero;

        public Vector3 PointLightPosition { get; set; } = Vector3.Zero;

        public Vector3 PointLightColor { get; set; } = Vector3.Zero;

        public float PointLightPower { get; set; } = 0f;

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
        }

        /// <inheritdoc />
        public void ContextCreated()
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
                PrimitiveType.Triangles,
                this.capacity,
                new[] { 0, 1, 2 },
                GlVertexArrayObject.MakeVertexArrayObject);

            this.coloredLineBuffer = new ReactiveBuffer<PrimitiveVertex>(
                this.source.Select(pso =>
                    pso.Select(ps =>
                        ps.Where(p => !p.IsTrianglePrimitive).SelectMany(p => p.Vertices).ToList())),
                PrimitiveType.Lines,
                this.capacity,
                new[] { 0, 1, },
                GlVertexArrayObject.MakeVertexArrayObject);
        }

        /// <inheritdoc />
        public void ContextUpdate(TimeSpan elapsed)
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
                AmbientLightColor,
                DirectedLightDirection,
                DirectedLightColor,
                PointLightPosition,
                PointLightColor,
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
