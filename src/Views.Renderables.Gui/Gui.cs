namespace GLHDN.Views.Renderables.Gui
{
    using System.Numerics;
    using OpenGL;
    using GLHDN.Core;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Collections;
    using System.Linq;

    /// <summary>
    /// Renderable container for a set of graphical user interface elements.
    /// </summary>
    public class Gui : IRenderable, IElementParent
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.Gui";

        private readonly View view;

        private GlProgramBuilder programBuilder;
        private GlProgram program;
        private ObservableCollection<Element> elements = new ObservableCollection<Element>();
        private BoundBuffer<Element, GuiVertex> guiElementBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Gui"/> class, 
        /// </summary>
        /// <param name="view"></param>
        public Gui(View view)
        {
            this.view = view;
            view.Renderables.Add(this);

            this.SubElements = new SubElementCollection(this);

            // TODO: allow program to be shared..
            this.programBuilder = new GlProgramBuilder()
                .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Gui.Vertex.glsl")
                .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Gui.Fragment.glsl")
                .WithUniforms("P");
        }

        public ICollection<Element> SubElements { get; }

        public Vector2 Center_ScreenSpace => Vector2.Zero;

        public Vector2 Size_ScreenSpace => new Vector2(view.Width, view.Height);

        /// <inheritdoc />
        public void ContextCreated(DeviceContext deviceContext)
        {
            this.program = this.programBuilder.Build();
            this.programBuilder = null;

            this.guiElementBuffer = new BoundBuffer<Element, GuiVertex>(
                elements,
                PrimitiveType.Triangles,
                100,
                a => a.Vertices,
                new[] { 0, 2, 3, 0, 3, 1 });

            var panel = new PanelElement()
            {
                ParentOrigin = new Dimensions(-1f, 0f),
                LocalOrigin = new Dimensions(-1f, 0f),
                Size = new Dimensions(200, 1f),
                Color = new Vector4(0.5f, 0.2f, 0.2f, 0.5f),
                BorderWidth = 1f
            };
            this.SubElements.Add(panel);
            panel.SubElements.Add(new TextElement("Fonts\\Inconsolata\\Inconsolata-Regular.ttf", "Hello world!")
            {
                ParentOrigin = new Dimensions(0f, 0f),
                LocalOrigin = new Dimensions(0f, 0f),
                Size = new Dimensions(1f, 1f),
                Color = new Vector4(1f, 1f, 1f, 1f)
            });
        }

        /// <inheritdoc />
        public void Render(DeviceContext deviceContext)
        {
            this.program.UseWithUniformValues(Matrix4x4.Transpose(Matrix4x4.CreateOrthographic(Size_ScreenSpace.X, Size_ScreenSpace.Y, 1f, -1f)));
            this.guiElementBuffer.Draw();
        }

        /// <inheritdoc />
        public void ContextDestroying(DeviceContext deviceContext)
        {
            this.program.Dispose();
            this.guiElementBuffer.Dispose();
        }

        public void Update()
        {
            // If size has changed, we need to recalculate the position of anything that's relatively positioned
        }

        /// <remarks>
        /// Ultimately to work nicely with our BoundBuffer class, we need to flatten the elements in the single collection
        /// in the <see cref="Gui"/> instance. So that collection ultimately backs this one, which just provides a view on
        /// it consisting of all the elements with a particular parent element.
        /// </remarks>
        private class SubElementCollection : ICollection<Element>
        {
            private Gui owner;

            public SubElementCollection(Gui owner)
            {
                this.owner = owner;
            }

            public int Count => throw new System.NotImplementedException();

            public bool IsReadOnly => throw new System.NotImplementedException();

            public void Add(Element element)
            {
                element.Parent = element.Parent ?? this.owner;
                owner.elements.Add(element);
            }

            public bool Remove(Element element)
            {
                return owner.elements.Remove(element);
            }

            public void Clear()
            {
                owner.elements.Clear(); // todo only this
            }

            public bool Contains(Element item)
            {
                throw new System.NotImplementedException();
            }

            public void CopyTo(Element[] array, int arrayIndex)
            {
                throw new System.NotImplementedException();
            }

            /// <inheritdoc />
            public IEnumerator<Element> GetEnumerator()
            {
                return owner.elements
                    .Where(e => e.Parent == this.owner)
                    .GetEnumerator();
            }

            /// <inheritdoc />)
            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)owner.elements
                    .Where(e => e.Parent == this.owner))
                    .GetEnumerator();
            }
        }
    }
}
