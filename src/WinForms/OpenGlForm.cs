namespace OpenGlHelpers.WinForms
{
    using System;
    using System.Windows.Forms;
    using OpenGL;
    using System.Diagnostics;
    using OpenGlHelpers.Core;
    using System.Drawing;
    using System.Collections.Generic;

    /// <summary>
    /// Windows form containing only a single OpenGL render control, with handlers as passed to the forms constructor.
    /// </summary>
    public sealed class OpenGlForm : Form, IUiContext
    {
        private readonly ICamera camera;
        private readonly Stopwatch modelUpdateIntervalStopwatch = new Stopwatch();
        private readonly Timer modelUpdateTimer; // TODO: this is the wrong timer type to use..
        private readonly Action<TimeSpan> modelUpdateHandler;
        private readonly bool lockCursor;

        public OpenGlForm(IRenderer[] renderers, ICamera camera, Action<TimeSpan> modelUpdateHandler, bool lockCursor)
        {
            Scene scene = new Scene(renderers);
            this.camera = camera;

            this.SuspendLayout();

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.Name = "OpenGl";
            this.Text = "OpenGl";

            Gl.DebugMessageCallback(HandleDebugMessage, null);
            this.GlControl = new GlControl();
            this.GlControl.Animation = true;
            this.GlControl.BackColor = Color.DimGray;
            this.GlControl.ColorBits = ((uint)(24u));
            this.GlControl.DepthBits = ((uint)(8u));
            this.GlControl.Dock = DockStyle.Fill;
            this.GlControl.Location = new Point(0, 0);
            this.GlControl.Margin = new Padding(0,0,0,0);
            this.GlControl.MultisampleBits = ((uint)(0u));
            this.GlControl.Name = "RenderControl";
            this.GlControl.StencilBits = ((uint)(0u));
            this.GlControl.TabIndex = 0;
            this.GlControl.ContextCreated += (s, a) => scene.ContextCreated(a.DeviceContext);
            this.GlControl.Render += (s, a) => scene.Render(a.DeviceContext, camera.ViewMatrix, camera.ProjectionMatrix);
            this.GlControl.ContextUpdate += (s, a) => scene.ContextUpdate(a.DeviceContext);
            this.GlControl.ContextDestroying += (s, a) => scene.ContextDestroying(a.DeviceContext);
            if (lockCursor)
            {
                this.lockCursor = true;
                this.GlControl.GotFocus += (s, a) => Cursor.Position = GlControl.PointToScreen(new Point(GlControl.Width / 2, GlControl.Height / 2));
            }
            this.GlControl.PreviewKeyDown += GlControl_KeyDown;
            this.GlControl.KeyUp += GlControl_KeyUp;
            this.GlControl.MouseWheel += GlControl_MouseWheel;
            this.GlControl.MouseUp += GlControl_MouseUp;
            this.Controls.Add(this.GlControl);

            this.modelUpdateTimer = new Timer();
            this.modelUpdateTimer.Interval = 15;
            this.modelUpdateTimer.Tick += new EventHandler(this.OnTimerTick);

            this.ResumeLayout(false);

            this.modelUpdateHandler = modelUpdateHandler;
            this.modelUpdateTimer.Start();
        }

        public GlControl GlControl { get; private set; }

        /// <inheritdoc />
        public int DisplayWidth => GlControl.Width;

        /// <inheritdoc />
        public int DisplayHeight => GlControl.Height;

        /// <inheritdoc />
        public HashSet<char> PressedKeys { get; private set; } = new HashSet<char>();

        /// <inheritdoc />
        public int CursorMovementX { get; private set; }

        /// <inheritdoc />
        public int CursorMovementY { get; private set; }

        /// <inheritdoc />
        public int MouseWheelDelta { get; private set; }

        /// <inheritdoc />
        public bool MouseButtonReleased { get; private set; }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                modelUpdateTimer?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (GlControl.Focused)
            {
                // Get mouse movement then reset      
                var cursorPos = GlControl.PointToClient(Cursor.Position);
                CursorMovementX = (GlControl.Width / 2) - cursorPos.X;
                CursorMovementY = (GlControl.Height / 2) - cursorPos.Y;
                if (this.lockCursor)
                {
                    Cursor.Position = GlControl.PointToScreen(new Point(GlControl.Width / 2, GlControl.Height / 2));
                }

                // Record update interval and restart stopwatch for it
                var elapsed = modelUpdateIntervalStopwatch.Elapsed;
                modelUpdateIntervalStopwatch.Restart();

                // Cap the effective elapsed time so that at worst,
                // the action will slow down as opposed to stuff jumping through walls..
                var maxEffectiveElapsed = TimeSpan.FromSeconds(0.1);
                if (elapsed > maxEffectiveElapsed) { elapsed = maxEffectiveElapsed; }

                // Update the game world, timing how long it takes to execute
                //updateDurationStopwatch.Restart();
                this.camera.Update(elapsed, this);
                this.modelUpdateHandler?.Invoke(elapsed);

                MouseWheelDelta = 0;
                MouseButtonReleased = false;
            }
        }

        private void GlControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                ActiveControl = null;
            }

            PressedKeys.Remove((char)e.KeyValue);
        }

        private void GlControl_KeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            PressedKeys.Add((char)e.KeyValue);
        }

        private void GlControl_MouseWheel(object sender, MouseEventArgs e)
        {
            MouseWheelDelta = e.Delta; // SO much is wrong with this approach..
        }

        private void GlControl_MouseUp(object sender, MouseEventArgs e)
        {
            MouseButtonReleased = true;
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