namespace GLHDN.Views.Renderables.Gui
{
    using GLHDN.Core;
    using GLHDN.ReactiveBuffers;
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Numerics;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    /// <summary>
    /// Renderable container for a set of graphical user interface elements.
    /// </summary>
    public class Gui : IRenderable, IElementParent
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.Gui.Shaders";

        private static object stateLock = new object();
        private static GlProgramBuilder programBuilder;
        private static GlProgram program;

        private readonly View view;

        private ReactiveBuffer<Vertex> vertexBuffer;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Gui"/> class, 
        /// </summary>
        /// <param name="view">The view from which to derive size and input.</param>
        public Gui(View view)
        {
            this.view = view;

            view.Resized += (s, e) =>
            {
                foreach (var element in SubElements)
                {
                    element.OnPropertyChanged(nameof(element.Parent));
                }
            };

            SubElements = new ElementCollection(this);

            if (program == null && programBuilder == null)
            {
                lock (stateLock)
                {
                    if (program == null && programBuilder == null)
                    {
                        programBuilder = new GlProgramBuilder()
                            .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Gui.Vertex.glsl")
                            .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Gui.Fragment.glsl")
                            .WithUniforms("P", "text");
                    }
                }
            }
        }

        /// <inheritdoc /> from IElementParent
        public ElementCollection SubElements { get; }

        /// <inheritdoc /> from IElementParent
        public Vector2 Center => Vector2.Zero;

        /// <inheritdoc /> from IElementParent
        public Vector2 Size => new Vector2(view.Width, view.Height);

        /// <inheritdoc /> from IRenderable
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

            this.vertexBuffer = new ReactiveBuffer<Vertex>(
                this.SubElements.Flatten(),
                PrimitiveType.Triangles,
                1000,
                new[] { 0, 2, 3, 0, 3, 1 },
                GlVertexArrayObject.MakeVertexArrayObject);
        }

        /// <inheritdoc />
        public void ContextUpdate(TimeSpan elapsed)
        {
            ThrowIfDisposed();

            void visitElement(ElementBase element)
            {
                if (element.Contains(new Vector2(view.CursorPosition.X, -view.CursorPosition.Y)))
                {
                    element.OnClicked(view.CursorPosition);

                    if (element is IElementParent parent)
                    {
                        foreach (var subElement in parent.SubElements)
                        {
                            visitElement(subElement);
                        }
                    }
                }
            }

            if (view.WasLeftMouseButtonReleased)
            {
                foreach (var element in this.SubElements)
                {
                    visitElement(element);
                }
            }
        }

        /// <inheritdoc /> from IRenderable
        public void Render()
        {
            ThrowIfDisposed();

            // Assume the GUI is drawn last and is independent - goes on top of everything drawn already - so clear the depth buffer
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            program.UseWithUniformValues(Matrix4x4.Transpose(Matrix4x4.CreateOrthographic(Size.X, Size.Y, 1f, -1f)), 0);
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2dArray, TextElement.font.Value.TextureId);
            this.vertexBuffer.Draw();
        }

        public void Dispose()
        {
            this.vertexBuffer?.Dispose();
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
