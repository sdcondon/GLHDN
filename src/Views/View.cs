﻿namespace GLHDN.Views
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
        private readonly Vector3 clearColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class.
        /// </summary>
        public View(
            IViewContext context,
            Action<TimeSpan> modelUpdateHandler,
            bool lockCursor,
            Vector3 clearColor)
        {
            Gl.DebugMessageCallback(OnGlDebugMessage, null);

            this.context = context;
            context.GlContextCreated += OnContextCreated;
            context.GlRender += OnRender;
            context.GlContextUpdate += OnContextUpdate;
            context.GlContextDestroying += OnContextDestroying;
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
            context.Update += OnUpdate;

            this.modelUpdateHandler = modelUpdateHandler;

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
        /// Gets the set of keys pressed since the last update.
        /// </summary>
        public HashSet<char> KeysPressed { get; private set; } = new HashSet<char>();

        /// <summary>
        /// Gets the set of currently pressed keys.
        /// </summary>
        public HashSet<char> KeysDown { get; private set; } = new HashSet<char>();

        /// <summary>
        /// Gets the set of keys released since the last update.
        /// </summary>
        public HashSet<char> KeysReleased { get; private set; } = new HashSet<char>();

        /// <summary>
        /// Gets the cursor position, with the origin being at the centre of the view.
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

        public event EventHandler<Vector2> Resized;

        private void OnContextCreated(object sender, DeviceContext context)
        {
            Gl.ClearColor(clearColor.X, clearColor.Y, clearColor.Z, 1f);
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
        
        private void OnRender(object sender, DeviceContext context)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            for (int i = 0; i < Renderables.Count; i++)
            {
                // Assume each renderable is independent - goes on top of everything drawn already - so clear the depth buffer
                Gl.Clear(ClearBufferMask.DepthBufferBit);
                Renderables[i].Render(context);
            }
        }

        private void OnContextUpdate(object sender, DeviceContext context)
        {
        }

        private void OnContextDestroying(object sender, DeviceContext context)
        {
            for (int i = 0; i < Renderables.Count; i++)
            {
                Renderables[i].ContextDestroying(context);
            }
        }

        private void OnUpdate(object sender, EventArgs e)
        {
            if (context.IsFocused)
            {
                // Get mouse movement   
                this.CursorPosition = context.GetCenter() - context.CursorPosition;

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
