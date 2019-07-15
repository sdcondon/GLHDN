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
        /// Loads the instance, compiling any programs and populating any required buffers.
        /// </summary>
        void Load();

        /// <summary>
        /// Performs an incremental update.
        /// </summary>
        void Update(TimeSpan elapsed);

        /// <summary>
        /// Render logic.
        /// </summary>
        void Render();
    }
}
