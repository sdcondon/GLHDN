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

    public class PrimitiveRenderer : IRenderable
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.Primitives";

        private readonly IViewProjection camera;
        private readonly IObservable<IObservable<IList<Primitive>>> source;

        private GlProgramBuilder programBuilder;
        private GlProgram program;
        private ReactiveBuffer<PrimitiveVertex> triangleBuffer;
        private ReactiveBuffer<PrimitiveVertex> lineBuffer;

        public Vector3 LightPosition { get; set; } = Vector3.Zero;

        public Vector3 LightColor { get; set; } = Vector3.Zero;

        public float LightPower { get; set; } = 0f;

        public Vector3 AmbientLightColor { get; set; } = Vector3.Zero;

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

            // TODO: allow program to be shared..
            this.programBuilder = new GlProgramBuilder()
                .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Colored.Vertex.glsl")
                .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Colored.Fragment.glsl")
                .WithUniforms("MVP", "V", "M", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor");
        }

        /// <inheritdoc />
        public void ContextCreated(DeviceContext deviceContext)
        {
            this.program = this.programBuilder.Build();
            this.programBuilder = null;

            this.triangleBuffer = new ReactiveBuffer<PrimitiveVertex>(
                this.source.Select(pso => 
                    pso.Select(ps => 
                        ps.Where(p => p.IsTrianglePrimitive).SelectMany(p => p.Vertices).ToList())),
                PrimitiveType.Triangles,
                1000,
                new[] { 0, 1, 2 },
                GlVertexArrayObject.MakeVertexArrayObject);

            this.lineBuffer = new ReactiveBuffer<PrimitiveVertex>(
                this.source.Select(pso =>
                    pso.Select(ps =>
                        ps.Where(p => !p.IsTrianglePrimitive).SelectMany(p => p.Vertices).ToList())),
                PrimitiveType.Lines,
                1000,
                new[] { 0, 1, },
                GlVertexArrayObject.MakeVertexArrayObject);
        }

        /// <inheritdoc />
        public void ContextDestroying(DeviceContext deviceContext)
        {
            triangleBuffer.Dispose();
            lineBuffer.Dispose();
            program.Dispose();
        }

        /// <inheritdoc />
        public void Render(DeviceContext deviceContext)
        {
            this.program.UseWithUniformValues(
                Matrix4x4.Transpose(this.camera.View * this.camera.Projection),
                Matrix4x4.Transpose(this.camera.View),
                Matrix4x4.Transpose(Matrix4x4.Identity),
                LightPosition,
                LightColor,
                LightPower,
                AmbientLightColor);
            this.triangleBuffer.Draw();
            this.lineBuffer.Draw();
        }
    }
}
