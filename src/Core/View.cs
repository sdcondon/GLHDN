namespace OpenGlHelpers.Core
{
    using OpenGL;
    using System.Numerics;

    public sealed class View
    {
        private IRenderer[] renderers;

        public View(params IRenderer[] renderers)
        {
            this.renderers = renderers;
        }

        /// <inheritdoc />
        public void ContextCreated(DeviceContext context)
        {
            Gl.ClearColor(0.0f, 0.0f, 0.1f, 0.0f); // Dark blue background
            Gl.Enable(EnableCap.DepthTest); // Enable depth test
            Gl.DepthFunc(DepthFunction.Less); // Accept fragment if it closer to the camera than the former one
            Gl.Enable(EnableCap.CullFace); // Cull triangles which normal is not towards the camera

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].ContextCreated(context);
            }
        }

        /// <inheritdoc />
        public void Render(DeviceContext context, Matrix4x4 view, Matrix4x4 projection)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].Render(context, Matrix4x4.Identity, view, projection);
            }
        }

        /// <inheritdoc />
        public void ContextUpdate(DeviceContext context)
        {
        }

        /// <inheritdoc />
        public void ContextDestroying(DeviceContext context)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].ContextDestroying(context);
            }
        }
    }
}
