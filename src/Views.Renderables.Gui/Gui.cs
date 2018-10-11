namespace GLHDN.Views.Renderables.Gui
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using OpenGL;
    using GLHDN.Core;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    /// <summary>
    /// Renderable container for a set of graphical user interface elements.
    /// </summary>
    public class Gui : IRenderable
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Renderables.Gui";

        private readonly View view;
        private readonly Queue<Action> updates = new Queue<Action>();
        private readonly RootElement rootElement;
        private readonly ObservableCollection<GuiElement> elements = new ObservableCollection<GuiElement>();

        private GlProgramBuilder programBuilder;
        private GlProgram program;
        private BoundBuffer<GuiElement, GuiVertex> guiElementBuffer;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Gui"/> class, 
        /// </summary>
        /// <param name="view"></param>
        public Gui(View view)
        {
            this.view = view;
            view.Renderables.Add(this);

            // TODO: allow program to be shared..
            this.programBuilder = new GlProgramBuilder()
                .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Gui.Vertex.glsl")
                .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Gui.Fragment.glsl")
                .WithUniforms("P");
        }

        /// <inheritdoc />
        public void ContextCreated(DeviceContext deviceContext)
        {
            this.program = this.programBuilder.Build();
            this.programBuilder = null;
            this.guiElementBuffer = new BoundBuffer<GuiElement, GuiVertex>(
                elements,
                PrimitiveType.Triangles,
                4,
                100,
                a => a.Vertices,
                new[] { 0, 2, 3, 0, 3, 1 });
        }

        /// <inheritdoc />
        public void Render(DeviceContext deviceContext)
        {
            // Apply updates
            // TODO: at least snapshot by switching the queues. and/or look into streaming?
            while (updates.Count > 0)
            {
                updates.Dequeue()();
            }

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
    }
}
