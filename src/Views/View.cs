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
    public sealed class View : IDisposable
    {
        private readonly IViewContext context;
        private readonly Color clearColor;
        private readonly Stopwatch modelUpdateIntervalStopwatch = new Stopwatch();
        private readonly HashSet<char> keysPressed = new HashSet<char>();
        private readonly HashSet<char> keysDown = new HashSet<char>();
        private readonly HashSet<char> keysReleased = new HashSet<char>();

        private IRenderable renderable;
        private bool lockCursor;
        private bool isContextCreated;

        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class.
        /// </summary>
        /// <param name="context">The <see cref="IViewContext"/> to use.</param>
        /// <param name="lockCursor">A value indicating whether the cursor is placed back at the center of the view during each update.</param>
        /// <param name="clearColor">The color to clear the view with on each render call.</param>
        public View(IViewContext context, bool lockCursor, Color clearColor)
        {
            Debug.WriteLine("Registering OpenGL debug handler");
            Gl.DebugMessageCallback(OnGlDebugMessage, null);

            KhronosApi.LogEnabled = true;
            KhronosApi.Log += KhronosApi_Log;

            this.context = context;
            context.GlContextCreated += OnGlContextCreated;
            context.GlRender += OnGlRender;
            context.GlContextUpdate += OnGlContextUpdate;
            context.GlContextDestroying += OnGlContextDestroying;
            context.KeyDown += OnKeyDown;
            context.KeyUp += OnKeyUp;
            context.MouseWheel += OnMouseWheel;
            context.LeftMouseDown += OnLeftMouseDown;
            context.LeftMouseUp += OnLeftMouseUp;
            context.RightMouseDown += OnRightMouseDown;
            context.RightMouseUp += OnRightMouseUp;
            context.MiddleMouseDown += OnMiddleMouseDown;
            context.MiddleMouseUp += OnMiddleMouseUp;
            context.Resize += OnResize;
            context.GotFocus += OnGotFocus;

            this.LockCursor = lockCursor;
            this.clearColor = clearColor;
        }

        /// <summary>
        /// An event that is fired when the size of the view changes. TODO: should be readonly.
        /// </summary>
        public event EventHandler<Vector2> Resized;

        /// <summary>
        /// Gets the set of keys pressed since the last update. TODO: should be readonly.
        /// </summary>
        public HashSet<char> KeysPressed => keysPressed;

        /// <summary>
        /// Gets the set of currently pressed keys. TODO: should be readonly.
        /// </summary>
        public HashSet<char> KeysDown => keysDown;

        /// <summary>
        /// Gets the set of keys released since the last update.
        /// </summary>
        public HashSet<char> KeysReleased => keysReleased;

        /// <summary>
        /// Gets the cursor position, with the origin being at the centre of the view, X increasing from left to right and Y increasing from top to bottom.
        /// </summary>
        public Vector2 CursorPosition { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the cursor is placed back at the center of the view during each update.
        /// </summary>
        public bool LockCursor
        {
            get => lockCursor;
            set
            {
                context.ShowCursor = !value;
                lockCursor = value;
            }
        }

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
        /// Gets or sets the root renderable of the view.
        /// </summary>
        public IRenderable Renderable
        {
            get => renderable;
            set
            {
                if (isContextCreated)
                {
                    value.Load();
                }

                renderable = value;
            }
        }

        /// <summary>
        /// Closes the view.
        /// </summary>
        public void Exit()
        {
            context.Exit();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            context.GlContextCreated -= OnGlContextCreated;
            context.GlRender -= OnGlRender;
            context.GlContextUpdate -= OnGlContextUpdate;
            context.GlContextDestroying -= OnGlContextDestroying;
            context.KeyDown -= OnKeyDown;
            context.KeyUp -= OnKeyUp;
            context.MouseWheel -= OnMouseWheel;
            context.LeftMouseDown -= OnLeftMouseDown;
            context.LeftMouseUp -= OnLeftMouseUp;
            context.RightMouseDown -= OnRightMouseDown;
            context.RightMouseUp -= OnRightMouseUp;
            context.MiddleMouseDown -= OnMiddleMouseDown;
            context.MiddleMouseUp -= OnMiddleMouseUp;
            context.Resize -= OnResize;

            if (this.lockCursor)
            {
                context.GotFocus -= OnGotFocus;
            }

            Renderable.Dispose();
        }

        private void OnGlContextCreated(object sender, DeviceContext context)
        {
            Gl.ClearColor(clearColor.R, clearColor.G, clearColor.B, clearColor.A);
            Gl.Enable(EnableCap.DepthTest); // Enable depth test
            Gl.DepthFunc(DepthFunction.Lequal); // Accept fragment if it closer to the camera than the former one
            Gl.Enable(EnableCap.CullFace); // Cull triangles of which normal is not towards the camera

            // Transparency
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            renderable.Load();
            isContextCreated = true;
        }

        private void OnGlRender(object sender, DeviceContext context)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderable.Render();
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
                if (elapsed > maxEffectiveElapsed)
                {
                    elapsed = maxEffectiveElapsed;
                }

                // Update the game world
                renderable.Update(elapsed);

                // Reset user input properties
                this.MouseWheelDelta = 0;
                this.WasLeftMouseButtonPressed = false;
                this.WasLeftMouseButtonReleased = false;
                this.WasRightMouseButtonPressed = false;
                this.WasRightMouseButtonReleased = false;
                this.WasMiddleMouseButtonPressed = false;
                this.WasMiddleMouseButtonReleased = false;
                this.keysPressed.Clear();
                this.keysReleased.Clear();
                if (this.lockCursor)
                {
                    context.CursorPosition = context.GetCenter();
                }
            }
        }

        private void OnGlContextDestroying(object sender, DeviceContext context)
        {
            Dispose();
        }

        private void OnKeyDown(object sender, char a)
        {
            keysPressed.Add(a);
            keysDown.Add(a);
        }

        private void OnKeyUp(object sender, char a)
        {
            keysReleased.Add(a);
            keysDown.Remove(a);
        }

        private void OnMouseWheel(object s, int a) => MouseWheelDelta = a; // SO much is wrong with this approach..

        private void OnLeftMouseDown(object s, EventArgs a)
        {
            WasLeftMouseButtonPressed = true;
            IsLeftMouseButtonDown = true;
        }

        private void OnLeftMouseUp(object s, EventArgs a)
        {
            WasLeftMouseButtonReleased = true;
            IsLeftMouseButtonDown = false;
        }

        private void OnRightMouseDown(object s, EventArgs a)
        {
            WasRightMouseButtonPressed = true;
            IsRightMouseButtonDown = true;
        }

        private void OnRightMouseUp(object s, EventArgs a)
        {
            WasRightMouseButtonReleased = true;
            IsRightMouseButtonDown = false;
        }

        private void OnMiddleMouseDown(object s, EventArgs a)
        {
            WasMiddleMouseButtonPressed = true;
            IsMiddleMouseButtonDown = true;
        }

        private void OnMiddleMouseUp(object s, EventArgs a)
        {
            WasMiddleMouseButtonReleased = true;
            IsMiddleMouseButtonDown = false;
        }

        private void OnResize(object sender, Vector2 size)
        {
            Gl.Viewport(0, 0, (int)size.X, (int)size.Y);
            Resized?.Invoke(this, size);
        }

        private void OnGotFocus(object sender, EventArgs a)
        {
            if (this.lockCursor)
            {
                context.CursorPosition = context.GetCenter();
            }
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

        private void KhronosApi_Log(object sender, Khronos.KhronosLogEventArgs e)
        {
            Debug.WriteLine($"{e.Name}({string.Join(',', e.Args)}) {e.ReturnValue}", "KHRONOS API");
        }
    }
}
