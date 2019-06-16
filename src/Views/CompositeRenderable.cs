using OpenGL;
using System.Collections.Generic;

namespace GLHDN.Views
{
    public abstract class CompositeRenderable : IRenderable
    {
        protected List<IRenderable> Renderables { get; } = new List<IRenderable>();

        public void ContextCreated(DeviceContext deviceContext)
        {
            for (int i = 0; i < Renderables.Count; i++)
            {
                Renderables[i].ContextCreated(deviceContext);
            }
        }

        public void ContextDestroying(DeviceContext deviceContext)
        {
            for (int i = 0; i < Renderables.Count; i++)
            {
                Renderables[i].ContextDestroying(deviceContext);
            }
        }

        public void Render(DeviceContext deviceContext)
        {
            for (int i = 0; i < Renderables.Count; i++)
            {
                Renderables[i].Render(deviceContext);
            }
        }
    }
}
