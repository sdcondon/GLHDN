namespace GLHDN.Views.Renderables.Gui
{
    using GLHDN.Core;
    using GLHDN.ReactiveBuffers;
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    /// <summary>
    /// Renderable container for a set of graphical user interface elements.
    /// </summary>
    public class Gui : IRenderable, IElementParent
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.Gui";

        private readonly View view;

        private GlProgramBuilder programBuilder;
        private GlProgram program;
        private BehaviorSubject<IElementParent> subject; 
        private ReactiveBuffer<GuiVertex> vertexBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Gui"/> class, 
        /// </summary>
        /// <param name="view"></param>
        public Gui(View view)
        {
            this.view = view;
            view.Renderables.Add(this);
            view.Resized += (s, e) =>
            {
                foreach (var element in SubElements)
                {
                    element.OnPropertyChanged("Parent");
                }
            };

            SubElements = new ElementCollection(this);

            // TODO: allow program to be shared..
            this.programBuilder = new GlProgramBuilder()
                .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Gui.Vertex.glsl")
                .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Gui.Fragment.glsl")
                .WithUniforms("P", "text");
        }

        /// <inheritdoc /> from IElementParent
        public ElementCollection SubElements { get; }

        /// <inheritdoc /> from IElementParent
        public Vector2 Center => Vector2.Zero;

        /// <inheritdoc /> from IElementParent
        public Vector2 Size => new Vector2(view.Width, view.Height);

        /// <inheritdoc /> from IRenderable
        public void ContextCreated(DeviceContext deviceContext)
        {
            this.program = this.programBuilder.Build();
            this.programBuilder = null;

            this.subject = new BehaviorSubject<IElementParent>(this);
            this.vertexBuffer = new ReactiveBuffer<GuiVertex>(
                this.subject.FlattenComposite<object, IList<GuiVertex>>(
                    a => a is IElementParent p ? p.SubElements : Observable.Never<IObservable<Element>>(),
                    a => a is Element e ? e.Vertices : new GuiVertex[0]),
                PrimitiveType.Triangles,
                1000,
                new[] { 0, 2, 3, 0, 3, 1 },
                GlVertexArrayObject.MakeVertexArrayObject);
        }

        /// <inheritdoc /> from IRenderable
        public void Render(DeviceContext deviceContext)
        {
            // Assume the GUI is drawn last and is independent - goes on top of everything drawn already - so clear the depth buffer
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            this.program.UseWithUniformValues(Matrix4x4.Transpose(Matrix4x4.CreateOrthographic(Size.X, Size.Y, 1f, -1f)), 0);
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2dArray, TextElement.font.TextureId);
            this.vertexBuffer.Draw();
        }

        /// <inheritdoc /> from IRenderable
        public void ContextDestroying(DeviceContext deviceContext)
        {
            this.subject?.OnCompleted();
            this.program?.Dispose();
            this.vertexBuffer?.Dispose();
        }

        public void Update()
        {
            void visitElement(Element element)
            {
                if (view.CursorPosition.X > element.PosTL.X
                    && view.CursorPosition.X < element.PosTR.X
                    && view.CursorPosition.Y < element.PosTL.Y
                    && view.CursorPosition.Y > element.PosBL.Y)
                {
                    element.OnClicked(view.CursorPosition);

                    foreach (var subElement in this.SubElements)
                    {
                        visitElement(subElement);
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
    }
}
