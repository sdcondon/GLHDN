namespace OpenGlHelpers.Core
{
    using OpenGL;

    /// <summary>
    /// A discrete renderable part of a <see cref="View"/>. Typically will encapsulate everything
    /// that results in one or more OpenGl draw calls: the program(s), the relevant buffers, etc.
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        /// Handler for render context creation.
        /// </summary>
        void ContextCreated(DeviceContext deviceContext);

        /// <summary>
        /// Render logic.
        /// </summary>
        void Render(DeviceContext deviceContext);

        /// <summary>
        /// Handler for the render context being destroyed.
        /// </summary>
        void ContextDestroying(DeviceContext deviceContext);
    }
}
