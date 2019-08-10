using System;
using System.Collections.Generic;

namespace GLHDN.Views
{
    /// <summary>
    /// Base class for <see cref="IRenderable"/> instances that are composed of sub-renderables.
    /// </summary>
    public abstract class CompositeRenderable : IRenderable, IDisposable
    {
        private readonly List<IRenderable> renderables = new List<IRenderable>();

        private bool contextCreated;

        /// <summary>
        /// Adds a sub-renderable.
        /// </summary>
        /// <param name="renderable">The sub-renderable to be added.</param>
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
