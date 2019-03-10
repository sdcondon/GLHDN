﻿namespace GLHDN.Views.Renderables.BasicExamples
{
    using OpenGL;
    using GLHDN.Core;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// Simple renderable class for static 3D geometry.
    /// </summary>
    public class ColoredStaticMesh : IRenderable
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.BasicExamples";

        private readonly IViewProjection viewProjection;

        private GlProgramBuilder programBuilder;
        private GlProgram program;
        private GlVertexArrayObjectBuilder vertexArrayObjectBuilder;
        private GlVertexArrayObject vertexArrayObject;

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

            // TODO: allow program to be shared..
            this.programBuilder = new GlProgramBuilder()
                .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Colored.Vertex.glsl")
                .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Colored.Fragment.glsl")
                .WithUniforms("MVP", "V", "M", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor");

            this.vertexArrayObjectBuilder = new GlVertexArrayObjectBuilder(PrimitiveType.Triangles)
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
        public void ContextCreated(DeviceContext deviceContext)
        {
            this.program = this.programBuilder.Build();
            this.programBuilder = null;
            this.vertexArrayObject = this.vertexArrayObjectBuilder.Build();
            this.vertexArrayObjectBuilder = null;
        }

        /// <inheritdoc />
        public void Render(DeviceContext deviceContext)
        {
            this.program.UseWithUniformValues(
                Matrix4x4.Transpose(this.Model * this.viewProjection.View * this.viewProjection.Projection),
                Matrix4x4.Transpose(this.viewProjection.View),
                Matrix4x4.Transpose(this.Model),
                new Vector3(4, 4, 4),
                new Vector3(1, 1, 1),
                30f,
                new Vector3(0.3f, 0.3f, 0.3f));
            this.vertexArrayObject.Draw();
        }

        /// <inheritdoc />
        public void ContextDestroying(DeviceContext deviceContext)
        {
            this.vertexArrayObject.Dispose();
            this.program.Dispose();
        }
    }
}