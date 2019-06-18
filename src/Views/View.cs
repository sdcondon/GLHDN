namespace GLHDN.Views
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Numerics;

    /// <summary>
    /// Encapsulates an interactive view rendered with OpenGl.
    /// </summary>
    public sealed class View
    {
        private readonly IViewContext context;
        private readonly Stopwatch modelUpdateIntervalStopwatch = new Stopwatch();
        private readonly bool lockCursor;
        private readonly Vector3 clearColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="lockCursor"></param>
        /// <param name="clearColor"></param>
        public View(IViewContext context, bool lockCursor, Vector3 clearColor)
        {
            Debug.WriteLine("Registering OpenGL debug handler");
            Gl.DebugMessageCallback(OnGlDebugMessage, null);

            this.context = context;
            context.GlContextCreated += OnGlContextCreated;
            context.GlRender += OnGlRender;
            context.GlContextUpdate += OnGlContextUpdate;
            context.GlContextDestroying += OnGlContextDestroying;
            context.KeyDown += (s, a) => { KeysPressed.Add(a); KeysDown.Add(a); };
            context.KeyUp += (s, a) => { KeysReleased.Add(a); KeysDown.Remove(a); };
            context.MouseWheel += (s, a) => MouseWheelDelta = a; // SO much is wrong with this approach..;
            context.LeftMouseDown += (s, a) => { WasLeftMouseButtonPressed = true; IsLeftMouseButtonDown = true; };
            context.LeftMouseUp += (s, a) => { WasLeftMouseButtonReleased = true; IsLeftMouseButtonDown = false; };
            context.RightMouseDown += (s, a) => { WasRightMouseButtonPressed = true; IsRightMouseButtonDown = true; };
            context.RightMouseUp += (s, a) => { WasRightMouseButtonReleased = true; IsRightMouseButtonDown = false; };
            context.MiddleMouseDown += (s, a) => { WasMiddleMouseButtonPressed = true; IsMiddleMouseButtonDown = true; };
            context.MiddleMouseUp += (s, a) => { WasMiddleMouseButtonReleased = true; IsMiddleMouseButtonDown = false; };
            context.Resize += OnResize;

            if (this.lockCursor = lockCursor)
            {
                context.GotFocus += (s, a) => context.CursorPosition = context.GetCenter();
                context.HideCursor();
            }

            this.clearColor = clearColor;
        }

        /// <summary>
        /// Gets the list of objects being rendered.
        /// </summary>
        public List<IRenderable> Renderables { get; private set; } = new List<IRenderable>();

        /// <summary>
        /// Gets the set of keys pressed since the last update. TODO: should be readonly
        /// </summary>
        public HashSet<char> KeysPressed { get; private set; } = new HashSet<char>();

        /// <summary>
        /// Gets the set of currently pressed keys. TODO: should be readonly
        /// </summary>
        public HashSet<char> KeysDown { get; private set; } = new HashSet<char>();

        /// <summary>
        /// Gets the set of keys released since the last update. TODO: should be readonly
        /// </summary>
        public HashSet<char> KeysReleased { get; private set; } = new HashSet<char>();

        /// <summary>
        /// Gets the cursor position, with the origin being at the centre of the view, X increasing from left to right and Y increasing from top to bottom.
        /// </summary>
        public Vector2 CursorPosition { get; private set; }

        /// <summary>
        /// Gets the mouse wheel delta since the last update.
        /// </summary>
        public int MouseWheelDelta { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the left mouse button has been pressed since the last update.
        /// </summary>
        public bool WasLeftMouseButtonPressed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the left mouse button is currently down.
        /// </summary>
        public bool IsLeftMouseButtonDown { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the left mouse button has been released since the last update.
        /// </summary>
        public bool WasLeftMouseButtonReleased { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the right mouse button has been pressed since the last update.
        /// </summary>
        public bool WasRightMouseButtonPressed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the right mouse button is currently down.
        /// </summary>
        public bool IsRightMouseButtonDown { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the right mouse button has been released since the last update.
        /// </summary>
        public bool WasRightMouseButtonReleased { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the middle mouse button has been pressed since the last update.
        /// </summary>
        public bool WasMiddleMouseButtonPressed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the middle mouse button is currently down.
        /// </summary>
        public bool IsMiddleMouseButtonDown { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the middle mouse button has been released since the last update.
        /// </summary>
        public bool WasMiddleMouseButtonReleased { get; private set; }

        /// <summary>
        /// Gets the width of the view.
        /// </summary>
        public int Width => context.Width;

        /// <summary>
        /// Gets the height of the view.
        /// </summary>
        public int Height => context.Height;

        /// <summary>
        /// Gets the aspect ratio of the view.
        /// </summary>
        public float AspectRatio => (float)context.Width / context.Height;

        /// <summary>
        /// An event that is fired when the size of the view changes.
        /// </summary>
        public event EventHandler<Vector2> Resized;

        /// <summary>
        /// An event that is fired periodically - whenever the view context updates.
        /// </summary>
        public event EventHandler<TimeSpan> Update;

        private void OnGlContextCreated(object sender, DeviceContext context)
        {
            Gl.ClearColor(clearColor.X, clearColor.Y, clearColor.Z, 0f);
            Gl.Enable(EnableCap.DepthTest); // Enable depth test
            Gl.DepthFunc(DepthFunction.Lequal); // Accept fragment if it closer to the camera than the former one
            Gl.Enable(EnableCap.CullFace); // Cull triangles of which normal is not towards the camera

            // Transparency
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            for (int i = 0; i < Renderables.Count; i++)
            {
                Renderables[i].ContextCreated(context);
            }
        }
        
        private void OnGlRender(object sender, DeviceContext context)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            for (int i = 0; i < Renderables.Count; i++)
            {
                Renderables[i].Render(context);
            }
        }

        private void OnGlContextUpdate(object sender, DeviceContext deviceContext)
        {
            if (context.IsFocused)
            {
                // Get mouse movement
                this.CursorPosition = context.CursorPosition - context.GetCenter();

                // Record update interval and restart stopwatch for it
                // Cap the effective elapsed time so that at worst,
                // the action will slow down as opposed to stuff jumping through walls..
                var elapsed = modelUpdateIntervalStopwatch.Elapsed;
                modelUpdateIntervalStopwatch.Restart();
                var maxEffectiveElapsed = TimeSpan.FromSeconds(0.1);
                if (elapsed > maxEffectiveElapsed) { elapsed = maxEffectiveElapsed; }

                // Update the game world, timing how long it takes to execute
                //updateDurationStopwatch.Restart();
                Update?.Invoke(this, elapsed);

                // Reset user input properties
                this.MouseWheelDelta = 0;
                this.WasLeftMouseButtonPressed = false;
                this.WasLeftMouseButtonReleased = false;
                this.WasRightMouseButtonPressed = false;
                this.WasRightMouseButtonReleased = false;
                this.WasMiddleMouseButtonPressed = false;
                this.WasMiddleMouseButtonReleased = false;
                this.KeysPressed.Clear();
                this.KeysReleased.Clear();
                if (this.lockCursor)
                {
                    context.CursorPosition = context.GetCenter();
                }
            }
        }

        private void OnGlContextDestroying(object sender, DeviceContext context)
        {
            for (int i = 0; i < Renderables.Count; i++)
            {
                Renderables[i].ContextDestroying(context);
            }
        }

        private void OnResize(object sender, Vector2 size)
        {
            Gl.Viewport(0, 0, (int)size.X, (int)size.Y);
            Resized?.Invoke(this, size);
        }

        private void OnGlDebugMessage(
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
