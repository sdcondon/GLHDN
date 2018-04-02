namespace OpenGlHelpers.Core
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Threading.Tasks;
    using OpenGL;
    using OpenGlHelpers.Core.LowLevel;

    /// <summary>
    /// Renderable container for graphical user interface elements.
    /// </summary>
    public class Gui : IRenderable
    {
        private const string ShaderResourceNamePrefix = "OpenGlHelpers.Core.Renderables.Gui";

        private readonly Queue<Action> updates = new Queue<Action>();

        private GlProgramBuilder programBuilder;
        private GlProgram program;
        private ObjectBufferBuilder<GuiElement> guiElementBufferBuilder;
        private ObjectBuffer<GuiElement> guiElementBuffer;

        public Gui()
        {
            // TODO: allow program to be shared..
            this.programBuilder = new GlProgramBuilder()
                .WithShaderFromEmbeddedResource(ShaderType.VertexShader, $"{ShaderResourceNamePrefix}.Gui.Vertex.glsl")
                .WithShaderFromEmbeddedResource(ShaderType.FragmentShader, $"{ShaderResourceNamePrefix}.Gui.Fragment.glsl")
                .WithUniforms("P");

            this.guiElementBufferBuilder = new ObjectBufferBuilder<GuiElement>(PrimitiveType.Triangles, 6, 100)
                .WithAttribute(a => new[]
                {
                    new Vector3(a.TopLeft, 0.5f),
                    new Vector3(a.BottomRight, 0.5f),
                    new Vector3(a.BottomLeft, 0.5f),
                    new Vector3(a.TopLeft, 0.5f),
                    new Vector3(a.TopRight, 0.5f),
                    new Vector3(a.BottomRight, 0.5f),
                })
                .WithAttribute(a => new[] { a.Color, a.Color, a.Color, a.Color, a.Color, a.Color });
        }

        public void AddElement(GuiElement element)
        {
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
            // TODO: look into streaming?
            while (updates.Count > 0)
            {
                updates.Dequeue()();
            }

            // Assume GUI goes on top of everything drawn already - so clear the depth buffer
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            // TODO: hard-code projection in shader?
            var projection = Matrix4x4.CreateOrthographic(2f, 2f, 1f, -1f);

            this.program.UseWithUniformValues(Matrix4x4.Transpose(projection));
            this.guiElementBuffer.Draw();
        }

        /// <inheritdoc />
        public void ContextDestroying(DeviceContext deviceContext)
        {
            this.program.Dispose();
            this.guiElementBuffer.Dispose();
        }
    }
}
