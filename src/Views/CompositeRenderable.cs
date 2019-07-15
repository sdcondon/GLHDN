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
                renderable.Load();
            }

            renderables.Add(renderable);
        }

        /// <inheritdoc />
        public void Load()
        {
            for (int i = 0; i < renderables.Count; i++)
            {
                renderables[i].Load();
            }

            contextCreated = true;
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
                renderables[i].Dispose();
            }
        }
    }
}
