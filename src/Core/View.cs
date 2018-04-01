namespace OpenGlHelpers.Core
{
    using OpenGL;
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Top-level class in this library. Encapsulates a view rendered with OpenGl.
    /// </summary>
    public sealed class View
    {
        private IRenderable[] renderables;

        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class.
        /// </summary>
        /// <param name="renderables"></param>
        public View(params IRenderable[] renderables)
        {
            Gl.DebugMessageCallback(HandleDebugMessage, null);

            this.renderables = renderables;
        }

        /// <summary>
        /// Handler to be invoked once the OpenGL context has been created.
        /// </summary>
        /// <param name="context"></param>
        public void ContextCreated(DeviceContext context)
        {
            Gl.ClearColor(0.0f, 0.0f, 0.1f, 0.0f); // Dark blue background
            Gl.Enable(EnableCap.DepthTest); // Enable depth test
            Gl.DepthFunc(DepthFunction.Less); // Accept fragment if it closer to the camera than the former one
            Gl.Enable(EnableCap.CullFace); // Cull triangles which normal is not towards the camera

            for (int i = 0; i < renderables.Length; i++)
            {
                renderables[i].ContextCreated(context);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="view"></param>
        /// <param name="projection"></param>
        public void Render(DeviceContext context)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            for (int i = 0; i < renderables.Length; i++)
            {
                renderables[i].Render(context);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void ContextUpdate(DeviceContext context)
        {
            // Gl.Viewport(0, 0, ((GlControl)s).Width, ((GlControl)s).Height) ?
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void ContextDestroying(DeviceContext context)
        {
            for (int i = 0; i < renderables.Length; i++)
            {
                renderables[i].ContextDestroying(context);
            }
        }

        private void HandleDebugMessage(
            Gl.DebugSource source,
            Gl.DebugType type,
            uint id,
            Gl.DebugSeverity severity,
            int length,
            IntPtr message,
            IntPtr userParam)
        {
            Debug.WriteLine($"{id} {source} {type} {severity}", "OPENGL");
        }
    }
}
