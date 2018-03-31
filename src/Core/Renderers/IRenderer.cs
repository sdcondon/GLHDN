namespace OpenGlHelpers.Core
{
    using OpenGL;
    using System.Numerics;

    public interface IRenderer
    {
        /// <summary>
        /// Handler for render context creation.
        /// </summary>
        void ContextCreated(DeviceContext deviceContext);

        /// <summary>
        /// Render logic.
        /// </summary>
        void Render(DeviceContext deviceContext, Matrix4x4 model, Matrix4x4 view, Matrix4x4 projection);

        /// <summary>
        /// Handler for the render context being destroyed.
        /// </summary>
        void ContextDestroying(DeviceContext deviceContext);
    }
}
