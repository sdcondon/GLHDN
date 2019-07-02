namespace GLHDN.Views
{
    using System;

    /// <summary>
    /// A discrete renderable part of a <see cref="View"/>. Typically will encapsulate everything
    /// that results in one or more OpenGl draw calls: the program(s), the relevant buffers, etc.
    /// </summary>
    public interface IRenderable : IDisposable
    {
        /// <summary>
        /// Handler for render context creation.
        /// </summary>
        void ContextCreated();

        /// <summary>
        /// Handler for render context updates.
        /// </summary>
        void ContextUpdate(TimeSpan elapsed);

        /// <summary>
        /// Render logic.
        /// </summary>
        void Render();
    }
}
