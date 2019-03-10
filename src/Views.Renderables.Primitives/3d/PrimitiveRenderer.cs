namespace GLHDN.Views.Renderables.Primitives._3d
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
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.Primitives._3d";

        private readonly IViewProjection camera;
        private readonly IObservable<IObservable<IList<Primitive>>> source;

        private GlProgramBuilder programBuilder;
        private GlProgram program;
        private ReactiveBuffer<PrimitiveVertex> primitiveBuffer;

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

            this.primitiveBuffer = new ReactiveBuffer<PrimitiveVertex>(
                this.source.Select(pso => pso.Select(p => p.SelectMany(y => y.Vertices).ToList())),
                PrimitiveType.Triangles,
                1000,
                new[] { 0, 1, 2 },
                GlVertexArrayObject.MakeVertexArrayObject);
        }

        /// <inheritdoc />
        public void ContextDestroying(DeviceContext deviceContext)
        {
            primitiveBuffer.Dispose();
            program.Dispose();
        }

        /// <inheritdoc />
        public void Render(DeviceContext deviceContext)
        {
            this.program.UseWithUniformValues(
                Matrix4x4.Transpose(this.camera.View * this.camera.Projection),
                Matrix4x4.Transpose(this.camera.View),
                Matrix4x4.Transpose(Matrix4x4.Identity),
                new Vector3(4, 4, 4),
                new Vector3(1, 1, 1),
                30f,
                new Vector3(0.2f, 0.2f, 0.2f));
            this.primitiveBuffer.Draw();
        }
    }
}
