namespace GLHDN.Views
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Numerics;

    /// <summary>
    /// Top-level class in this library. Encapsulates a view rendered with OpenGl.
    /// </summary>
    public sealed class View
    {
        private readonly IViewContext context;
        private readonly Stopwatch modelUpdateIntervalStopwatch = new Stopwatch();
        private readonly Action<TimeSpan> modelUpdateHandler;
        private readonly bool lockCursor;

        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class.
        /// </summary>
        public View(IViewContext context, Action<TimeSpan> modelUpdateHandler, bool lockCursor)
        {
            Gl.DebugMessageCallback(HandleDebugMessage, null);

            this.context = context;
            context.ContextCreated += ContextCreated;
            context.Render += Render;
            context.ContextUpdate += ContextUpdate;
            context.ContextDestroying += ContextDestroying;
            context.KeyDown += (s, a) => PressedKeys.Add(a);
            context.KeyUp += (s, a) => PressedKeys.Remove(a);
            context.MouseWheel += (s, a) => MouseWheelDelta = a; // SO much is wrong with this approach..;
            context.MouseUp += (s, a) => MouseButtonReleased = true;
            context.Resize += Resize;
            context.Update += Update;

            this.modelUpdateHandler = modelUpdateHandler;

            if (this.lockCursor = lockCursor)
            {
                context.GotFocus += (s, a) => context.CursorPosition = context.GetCenter();
                context.HideCursor();
            }
        }

        public List<IRenderable> Renderables { get; private set; } = new List<IRenderable>();

        public int Width => context.Width;
        public int Height => context.Height;
        public HashSet<char> PressedKeys { get; set; } = new HashSet<char>();
        public Vector2 CursorMovement { get; private set; }
        public int MouseWheelDelta { get; private set; }
        public bool MouseButtonReleased { get; private set; }

        public float AspectRatio => (float)context.Width / (float)context.Height;

        private void ContextCreated(object sender, DeviceContext context)
        {
            Gl.ClearColor(0.0f, 0.0f, 0.1f, 0.0f); // Dark blue background
            Gl.Enable(EnableCap.DepthTest); // Enable depth test
            Gl.DepthFunc(DepthFunction.Less); // Accept fragment if it closer to the camera than the former one
            Gl.Enable(EnableCap.CullFace); // Cull triangles which normal is not towards the camera

            //transparency
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            for (int i = 0; i < Renderables.Count; i++)
            {
                Renderables[i].ContextCreated(context);
            }
        }

        private void Render(object sender, DeviceContext context)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            for (int i = 0; i < Renderables.Count; i++)
            {
                // Assume each renderable is independent - goes on top of everything drawn already - so clear the depth buffer
                Gl.Clear(ClearBufferMask.DepthBufferBit);
                Renderables[i].Render(context);
            }
        }

        private void ContextUpdate(object sender, DeviceContext context)
        {
        }

        private void ContextDestroying(object sender, DeviceContext context)
        {
            for (int i = 0; i < Renderables.Count; i++)
            {
                Renderables[i].ContextDestroying(context);
            }
        }

        private void Update(object sender, EventArgs e)
        {
            if (context.IsFocused)
            {
                // Get mouse movement   
                this.CursorMovement = context.GetCenter() - context.CursorPosition;

                // Record update interval and restart stopwatch for it
                // Cap the effective elapsed time so that at worst,
                // the action will slow down as opposed to stuff jumping through walls..
                var elapsed = modelUpdateIntervalStopwatch.Elapsed;
                modelUpdateIntervalStopwatch.Restart();
                var maxEffectiveElapsed = TimeSpan.FromSeconds(0.1);
                if (elapsed > maxEffectiveElapsed) { elapsed = maxEffectiveElapsed; }

                // Update the game world, timing how long it takes to execute
                //updateDurationStopwatch.Restart();
                this.modelUpdateHandler?.Invoke(elapsed);

                // Reset user input properties
                this.MouseWheelDelta = 0;
                this.MouseButtonReleased = false;
                if (this.lockCursor)
                {
                    context.CursorPosition = context.GetCenter();
                }
            }
        }

        private void Resize(object sender, Vector2 size)
        {
            Gl.Viewport(0, 0, (int)size.X, (int)size.Y);
            // renderers need to handle?
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
