using OpenGL;
using System;
using System.Collections.Generic;

namespace GLHDN.Views
{
    public abstract class CompositeRenderable : IRenderable, IDisposable
    {
        protected List<IRenderable> Renderables { get; } = new List<IRenderable>();

        /// <inheritdoc />
        public void ContextCreated(DeviceContext deviceContext)
        {
            for (int i = 0; i < Renderables.Count; i++)
            {
                Renderables[i].ContextCreated(deviceContext);
            }
        }

        /// <inheritdoc />
        public void Render(DeviceContext deviceContext)
        {
            for (int i = 0; i < Renderables.Count; i++)
            {
                Renderables[i].Render(deviceContext);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            for (int i = 0; i < Renderables.Count; i++)
            {
                if (Renderables[i] is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
