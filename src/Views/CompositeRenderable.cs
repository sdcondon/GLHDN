using OpenGL;
using System;
using System.Collections.Generic;

namespace GLHDN.Views
{
    public abstract class CompositeRenderable : IRenderable, IDisposable
    {
        private readonly List<IRenderable> renderables = new List<IRenderable>();

        private bool contextCreated;

        public void AddRenderable(IRenderable renderable)
        {
            if (contextCreated)
            {
                renderable.ContextCreated();
            }

            renderables.Add(renderable);
        }

        /// <inheritdoc />
        public void ContextCreated()
        {
            for (int i = 0; i < renderables.Count; i++)
            {
                renderables[i].ContextCreated();
            }

            contextCreated = true;
        }

        /// <inheritdoc />
        public virtual void ContextUpdate(TimeSpan elapsed)
        {
            for (int i = 0; i < renderables.Count; i++)
            {
                renderables[i].ContextUpdate(elapsed);
            }
        }

        /// <inheritdoc />
        public void Render()
        {
            for (int i = 0; i < renderables.Count; i++)
            {
                renderables[i].Render();
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
