using OpenGL;
using System;
using System.Collections.Generic;

namespace GLHDN.Views
{
    public abstract class CompositeRenderable : IRenderable, IDisposable
    {
        private readonly List<IRenderable> renderables = new List<IRenderable>();

        private DeviceContext createdContext;

        public void AddRenderable(IRenderable renderable)
        {
            if (createdContext != null)
            {
                renderable.ContextCreated(createdContext);
            }

            renderables.Add(renderable);
        }

        /// <inheritdoc />
        public void ContextCreated(DeviceContext deviceContext)
        {
            for (int i = 0; i < renderables.Count; i++)
            {
                renderables[i].ContextCreated(deviceContext);
            }

            createdContext = deviceContext;
        }

        /// <inheritdoc />
        public void Render(DeviceContext deviceContext)
        {
            for (int i = 0; i < renderables.Count; i++)
            {
                renderables[i].Render(deviceContext);
            }
        }

        /// <inheritdoc />
        public virtual void Update(TimeSpan elapsed)
        {
            for (int i = 0; i < renderables.Count; i++)
            {
                renderables[i].Update(elapsed);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            for (int i = 0; i < renderables.Count; i++)
            {
                if (renderables[i] is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
