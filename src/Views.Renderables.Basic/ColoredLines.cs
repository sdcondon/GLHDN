namespace GLHDN.Views.Renderables.Basic
{
    using OpenGL;
    using GLHDN.Core;
    using System;
    using System.Numerics;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    /// <summary>
    /// Renderable class for 3D lines. For debug utilities.
    /// </summary>
    public class ColoredLines : IRenderable
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.Basic";

        private readonly IViewProjection viewProjection;
        private readonly ObservableCollection<Line> lines;

        private GlProgramBuilder programBuilder;
        private GlProgram program;
        private BoundBuffer<Line, Vertex> linesBuffer;

        public ColoredLines(IViewProjection viewProjection)
        {
            this.viewProjection = viewProjection;
            this.lines = new ObservableCollection<Line>();

            // TODO: allow program to be shared..
            this.programBuilder = new GlProgramBuilder()
                .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Colored.Vertex.glsl")
                .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Colored.Fragment.glsl")
                .WithUniforms("MVP", "V", "M", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor");
        }

        public void AddLine(Vector3 start, Vector3 end)
        {
            this.lines.Add(new Line(start, end));
        }

        public void ClearLines()
        {
            this.lines.Clear();
        }

        /// <inheritdoc />
        public void ContextCreated(DeviceContext deviceContext)
        {
            this.program = this.programBuilder.Build();
            this.programBuilder = null;
            this.linesBuffer = new BoundBuffer<Line, Vertex>(
                lines,
                PrimitiveType.Lines,
                2,
                100,
                a => new[] { new Vertex(a.from, Vector3.One, a.from), new Vertex(a.to, Vector3.One, a.to) },
                new[] { 0, 1 }); // TODO: Change so not needed
        }

        /// <inheritdoc />
        public void Render(DeviceContext deviceContext)
        {
            this.program.UseWithUniformValues(
                Matrix4x4.Transpose(this.viewProjection.View * this.viewProjection.Projection),
                Matrix4x4.Transpose(this.viewProjection.View),
                Matrix4x4.Identity,
                new Vector3(4, 4, 4),
                new Vector3(1, 1, 1),
                30f,
                new Vector3(0.3f, 0.3f, 0.3f));
            this.linesBuffer.Draw();
        }

        /// <inheritdoc />
        public void ContextDestroying(DeviceContext deviceContext)
        {
            this.linesBuffer.Dispose();
            this.program.Dispose();
        }

        private class Line : INotifyPropertyChanged
        {
            public readonly Vector3 from;
            public readonly Vector3 to;

            public Line(Vector3 from, Vector3 to)
            {
                this.from = from;
                this.to = to;
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private struct Vertex
        {
            public readonly Vector3 position;
            public readonly Vector3 color;
            public readonly Vector3 normal;
            
            public Vertex(Vector3 position, Vector3 color, Vector3 normal)
            {
                this.position = position;
                this.color = color;
                this.normal = normal;
            }
        }
    }
}
