namespace GLHDN.Views
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using OpenGL;
    using GLHDN.Core;

    /// <summary>
    /// Renderable container for graphical user interface elements.
    /// </summary>
    public class Gui : IRenderable, IGuiElement
    {
        private const string ShaderResourceNamePrefix = "GLHDN.Views.Gui";

        private readonly View view;
        private readonly Queue<Action> updates = new Queue<Action>();

        private GlProgramBuilder programBuilder;
        private GlProgram program;
        private ObjectBufferBuilder<GuiElement> guiElementBufferBuilder;
        private ObjectBuffer<GuiElement> guiElementBuffer;

        public Gui(View view, IEnumerable<GuiElement> elements)
        {
            this.view = view;
            view.Renderables.Add(this);

            // TODO: allow program to be shared..
            this.programBuilder = new GlProgramBuilder()
                .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Gui.Vertex.glsl")
                .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Gui.Fragment.glsl")
                .WithUniforms("P");

            this.guiElementBufferBuilder = new ObjectBufferBuilder<GuiElement>(PrimitiveType.Triangles, 4, 100)
                .WithAttribute(a => new[] { a.TopLeft(), a.TopRight(), a.BottomLeft(), a.BottomRight() })
                .WithAttribute(a => new[] { a.Color, a.Color, a.Color, a.Color })
                .WithAttribute(a => new[] { Vector2.Zero, new Vector2(a.ScreenSize.X, 0), new Vector2(0, a.ScreenSize.Y), a.ScreenSize })
                .WithAttribute(a => new[] { a.ScreenSize, a.ScreenSize, a.ScreenSize, a.ScreenSize })
                .WithAttribute(a => new[] { a.BorderWidth, a.BorderWidth, a.BorderWidth, a.BorderWidth })
                .WithIndices(new[] { 0, 2, 3, 0, 3, 1 });

            foreach (var element in elements)
            {
                AddElement(element);
            }
        }

        /// <inheritdoc />
        public Vector2 Center => Vector2.Zero;

        /// <inheritdoc />
        public Vector2 ScreenSize => new Vector2(view.Width, view.Height);

        /// <inheritdoc />
        public void ContextCreated(DeviceContext deviceContext)
        {
            this.program = this.programBuilder.Build();
            this.programBuilder = null;
            this.guiElementBuffer = this.guiElementBufferBuilder.Build();
            this.guiElementBufferBuilder = null;
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

            // Assume GUI goes on top of everything drawn already - so clear the depth buffer
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            this.program.UseWithUniformValues(Matrix4x4.Transpose(Matrix4x4.CreateOrthographic(ScreenSize.X, ScreenSize.Y, 1f, -1f)));
            this.guiElementBuffer.Draw();
        }

        /// <inheritdoc />
        public void ContextDestroying(DeviceContext deviceContext)
        {
            this.program.Dispose();
            this.guiElementBuffer.Dispose();
        }

        public void AddElement(GuiElement element)
        {
            element.Parent = element.Parent ?? this;
            updates.Enqueue(() => guiElementBuffer.Add(element));
        }

        public void RemoveElement(GuiElement element)
        {
            updates.Enqueue(() => guiElementBuffer.Remove(element));
        }

        public void Clear()
        {
            updates.Enqueue(() => guiElementBuffer.Clear());
        }

        public void Update()
        {
            // If size has changed, we need to recalculate the position of anything that's relatively positioned
        }
    }
}
