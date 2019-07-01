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

    public class PrimitiveRenderer : IRenderable, IDisposable
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.Primitives";

        private static object stateLock = new object();
        private static GlProgramBuilder programBuilder;
        private static GlProgram program;

        private readonly IViewProjection camera;
        private readonly IObservable<IObservable<IList<Primitive>>> source;

        private ReactiveBuffer<PrimitiveVertex> triangleBuffer;
        private ReactiveBuffer<PrimitiveVertex> lineBuffer;
        private bool isDisposed;

        public Vector3 AmbientLightColor { get; set; } = Vector3.Zero;

        public Vector3 DirectedLightDirection { get; set; } = Vector3.Zero;

        public Vector3 DirectedLightColor { get; set; } = Vector3.Zero;

        public Vector3 PointLightPosition { get; set; } = Vector3.Zero;

        public Vector3 PointLightColor { get; set; } = Vector3.Zero;

        public float PointLightPower { get; set; } = 0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveRenderer"/> class.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="source"></param>
        public PrimitiveRenderer(
            IViewProjection camera,
            IObservable<IObservable<IList<Primitive>>> source)
        {
            this.camera = camera;
            this.source = source;

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
        public void ContextCreated(DeviceContext deviceContext)
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

            this.triangleBuffer = new ReactiveBuffer<PrimitiveVertex>(
                this.source.Select(pso => 
                    pso.Select(ps => 
                        ps.Where(p => p.IsTrianglePrimitive).SelectMany(p => p.Vertices).ToList())),
                PrimitiveType.Triangles,
                100000,
                new[] { 0, 1, 2 },
                GlVertexArrayObject.MakeVertexArrayObject);

            this.lineBuffer = new ReactiveBuffer<PrimitiveVertex>(
                this.source.Select(pso =>
                    pso.Select(ps =>
                        ps.Where(p => !p.IsTrianglePrimitive).SelectMany(p => p.Vertices).ToList())),
                PrimitiveType.Lines,
                100000,
                new[] { 0, 1, },
                GlVertexArrayObject.MakeVertexArrayObject);
        }

        /// <inheritdoc />
        public void Render(DeviceContext deviceContext)
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
            this.triangleBuffer.Draw();
            this.lineBuffer.Draw();
        }

        /// <inheritdoc />
        public void Update(TimeSpan elapsed)
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            triangleBuffer.Dispose();
            lineBuffer.Dispose();
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
