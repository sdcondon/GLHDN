namespace GLHDN.Views.Renderables.BasicExamples
{
    using GLHDN.Core;
    using GLHDN.ReactiveBuffers;
    using OpenGL;
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Numerics;
    using System.Reactive.Linq;

    /// <summary>
    /// Renderable class for 3D lines. For debug utilities.
    /// </summary>
    public class ColoredLines : IRenderable
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.BasicExamples";

        private static readonly object ProgramStateLock = new object();
        private static ProgramBuilder programBuilder;
        private static GlProgram program;

        private readonly IViewProjection viewProjection;
        private readonly ObservableCollection<Line> lines;

        private ReactiveBufferBuilder<Vertex> linesBufferBuilder;
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

            this.linesBufferBuilder = new ReactiveBufferBuilder<Vertex>(
                PrimitiveType.Lines,
                100,
                new[] { 0, 1 },
                ((INotifyCollectionChanged)lines).ToObservable<Line>()
                    .Select(o => o
                        .Select(i => new[]
                        {
                            new Vertex(i.From, Color.White(), i.From),
                            new Vertex(i.To, Color.White(), i.To),
                        })));
        }

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

        /// <summary>
        /// Adds a line to be rendered.
        /// </summary>
        /// <param name="from">The position of the start of the line.</param>
        /// <param name="to">The position of the end of the line.</param>
        public void AddLine(Vector3 from, Vector3 to)
        {
            ThrowIfDisposed();

            this.lines.Add(new Line(from, to));
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
                lock (ProgramStateLock)
                {
                    if (program == null)
                    {
                        program = programBuilder.Build();
                        programBuilder = null;
                    }
                }
            }

            this.linesBuffer = linesBufferBuilder.Build();
            this.linesBufferBuilder = null;
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
                PointLightPosition,
                (Vector3)PointLightColor,
                PointLightPower,
                (Vector3)AmbientLightColor);
            this.linesBuffer.Draw();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.linesBuffer?.Dispose();
            isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private struct Vertex
        {
            public readonly Vector3 Position;
            public readonly Vector3 Color;
            public readonly Vector3 Normal;

            public Vertex(Vector3 position, Vector3 color, Vector3 normal)
            {
                this.Position = position;
                this.Color = color;
                this.Normal = normal;
            }
        }

        private class Line : INotifyPropertyChanged
        {
            private Vector3 from;
            private Vector3 to;

            public Line(Vector3 from, Vector3 to)
            {
                this.From = from;
                this.To = to;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public Vector3 From
            {
                get => from;
                set
                {
                    this.from = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(From)));
                }
            }

            public Vector3 To
            {
                get => to;
                set
                {
                    this.to = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(To)));
                }
            }
        }
    }
}
