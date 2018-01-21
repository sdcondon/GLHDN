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
    public sealed class OpenGlForm : Form, IContext
    {
        private Stopwatch modelUpdateIntervalStopwatch = new Stopwatch();
        private Timer modelUpdateTimer;
        private Action<TimeSpan> modelUpdateHandler;

        public OpenGlForm(IRenderer renderer, Action<TimeSpan> modelUpdateHandler)
        {
            this.SuspendLayout();

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.Name = "OpenGl";
            this.Text = "OpenGl";

            Gl.DebugMessageCallback(HandleDebugMessage, null);
            this.RenderControl = new GlControl();
            this.RenderControl.Animation = true;
            this.RenderControl.BackColor = Color.DimGray;
            this.RenderControl.ColorBits = ((uint)(24u));
            this.RenderControl.DepthBits = ((uint)(8u));
            this.RenderControl.Dock = DockStyle.Fill;
            this.RenderControl.Location = new Point(0, 0);
            this.RenderControl.Margin = new Padding(0,0,0,0);
            this.RenderControl.MultisampleBits = ((uint)(0u));
            this.RenderControl.Name = "RenderControl";
            this.RenderControl.StencilBits = ((uint)(0u));
            this.RenderControl.TabIndex = 0;
            this.RenderControl.ContextCreated += (s, a) => renderer.ContextCreated(s);
            this.RenderControl.Render += (s, a) => renderer.Render(s);
            this.RenderControl.ContextUpdate += (s, a) => renderer.ContextUpdate(s);
            this.RenderControl.ContextDestroying += (s, a) => renderer.ContextDestroying(s);
            this.RenderControl.GotFocus += (s, a) => Cursor.Position = RenderControl.PointToScreen(new Point(RenderControl.Width / 2, RenderControl.Height / 2));
            this.RenderControl.Cursor = Cursors.Cross;
            this.Controls.Add(this.RenderControl);

            this.modelUpdateTimer = new Timer();
            this.modelUpdateTimer.Interval = 15;
            this.modelUpdateTimer.Tick += new EventHandler(this.OnTimerTick);

            this.ResumeLayout(false);

            this.RenderControl.PreviewKeyDown += Control_KeyDown;
            this.RenderControl.KeyUp += Control_KeyUp;

            this.modelUpdateHandler = modelUpdateHandler;
            this.modelUpdateTimer.Start();
        }

        public GlControl RenderControl { get; private set; }

        /// <inheritdoc />
        public float DisplayAspectRatio => (float)RenderControl.Width / (float)RenderControl.Height;

        /// <inheritdoc />
        public HashSet<char> PressedKeys { get; private set; } = new HashSet<char>();

        /// <inheritdoc />
        public int CursorMovementX { get; private set; }

        /// <inheritdoc />
        public int CursorMovementY { get; private set; }

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
            if (RenderControl.Focused)
            {
                // Get mouse movement then reset      
                var cursorPos = RenderControl.PointToClient(Cursor.Position);
                Cursor.Position = RenderControl.PointToScreen(new Point(RenderControl.Width / 2, RenderControl.Height / 2));
                CursorMovementX = (RenderControl.Width / 2) - cursorPos.X;
                CursorMovementY = (RenderControl.Height / 2) - cursorPos.Y;

                // Record update interval and restart stopwatch for it
                var elapsed = modelUpdateIntervalStopwatch.Elapsed;
                //updateInterval = updateIntervalSmoother.Update(updateIntervalStopwatch.ElapsedMilliseconds);
                modelUpdateIntervalStopwatch.Restart();

                // Cap the effective elapsed time so that at worst,
                // the action will slow down as opposed to stuff jumping through walls..
                var maxEffectiveElapsed = TimeSpan.FromSeconds(0.1);
                if (elapsed > maxEffectiveElapsed) { elapsed = maxEffectiveElapsed; }

                // Update the game world, timing how long it takes to execute
                //updateDurationStopwatch.Restart();
                this.modelUpdateHandler?.Invoke(elapsed);
                //updateDuration = updateDurationSmoother.Update(updateDurationStopwatch.ElapsedMilliseconds);
            }
        }

        private void Control_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                ActiveControl = null;
            }

            PressedKeys.Remove((char)e.KeyValue);
        }

        private void Control_KeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            PressedKeys.Add((char)e.KeyValue);
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
            Debug.WriteLine($"OPENGL {id} {source} {type} {severity}");
        }
    }
}