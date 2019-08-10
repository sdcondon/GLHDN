namespace GLHDN.Views.Renderables.BasicExamples
{
    using GLHDN.Core;
    using GLHDN.ReactiveBuffers;
    using OpenGL;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Numerics;

    /// <summary>
    /// Renderable class for 3D lines. For debug utilities.
    /// </summary>
    public class ColoredLines : IRenderable
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.BasicExamples";

        private static readonly object stateLock = new object();
        private static GlProgramBuilder programBuilder;
        private static GlProgram program;

        private readonly IViewProjection viewProjection;
        private readonly ObservableCollection<Line> lines;

        private ReactiveBuffer<Vertex> linesBuffer;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredLines"/> class.
        /// </summary>
        /// <param name="viewProjection">The view and projection matrices to use when rendering.</param>
        public ColoredLines(IViewProjection viewProjection)
        {
            this.viewProjection = viewProjection;
            this.lines = new ObservableCollection<Line>();

            if (program == null && programBuilder == null)
            {
                lock (stateLock)
                {
                    if (program == null && programBuilder == null)
                    {
                        programBuilder = new GlProgramBuilder()
                            .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Colored.Vertex.glsl")
                            .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Colored.Fragment.glsl")
                            .WithUniforms("MVP", "V", "M", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor");
                    }
                }
            }
        }

        /// <summary>
        /// Adds a line to be rendered.
        /// </summary>
        /// <param name="start">The position of the start of the line.</param>
        /// <param name="end">The position of the end of the line.</param>
        public void AddLine(Vector3 start, Vector3 end)
        {
            ThrowIfDisposed();

            this.lines.Add(new Line(start, end));
        }

        /// <summary>
        /// Clears all of the lines.
        /// </summary>
        public void ClearLines()
        {
            ThrowIfDisposed();

            this.lines.Clear();
        }

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

            this.linesBuffer = new ReactiveBuffer<Vertex>(
                lines.ToObservable((Line a) => new[] { new Vertex(a.from, Vector3.One, a.from), new Vertex(a.to, Vector3.One, a.to) }),
                PrimitiveType.Lines,
                100,
                new[] { 0, 1 },
                GlVertexArrayObject.MakeVertexArrayObject);
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
        public void Dispose()
        {
            this.linesBuffer.Dispose();
            isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
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
