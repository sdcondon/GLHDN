namespace GLHDN.Views.Contexts.WinForms
{
    using GLHDN.Views;
    using OpenGL;
    using System;
    using System.Drawing;
    using System.Numerics;
    using System.Windows.Forms;

    /// <summary>
    /// Wrapper for <see cref="GlControl"/> to the <see cref="IViewContext"/> interface.
    /// </summary>
    public sealed class GlControlAdapter : IViewContext, IDisposable
    {
        private GlControl glControl;
        private readonly Timer modelUpdateTimer; // TODO: this is the wrong timer type to use - it's tied to forms update

        /// <summary>
        /// Initializes a new instance of the <see cref="GlControlAdapter"/> class.
        /// </summary>
        /// <param name="glControl">The <see cref="GlControl"/> to wrap.</param>
        public GlControlAdapter(GlControl glControl)
        {
            this.glControl = glControl;
            this.glControl.ContextCreated += (s, a) => ContextCreated?.Invoke(this, a.DeviceContext);
            this.glControl.Render += (s, a) => Render?.Invoke(this, a.DeviceContext);
            this.glControl.ContextUpdate += (s, a) => ContextUpdate?.Invoke(this, a.DeviceContext);
            this.glControl.ContextDestroying += (s, a) => ContextDestroying?.Invoke(this, a.DeviceContext);
            this.glControl.KeyDown += (s, a) => KeyDown?.Invoke(this, (char)a.KeyValue);
            this.glControl.KeyUp += (s, a) => KeyUp?.Invoke(this, (char)a.KeyValue);
            this.glControl.MouseWheel += (s, a) => MouseWheel?.Invoke(this, a.Delta);
            this.glControl.MouseUp += (s, a) => MouseUp?.Invoke(this, EventArgs.Empty);
            this.glControl.Resize += (s, a) => Resize?.Invoke(this, new Vector2(this.glControl.ClientSize.Width, this.glControl.ClientSize.Height));
            this.glControl.GotFocus += (s, a) => GotFocus?.Invoke(this, EventArgs.Empty);

            this.modelUpdateTimer = new Timer() { Interval = 15 };
            this.modelUpdateTimer.Tick += (s, a) => Update(this, EventArgs.Empty);
            this.modelUpdateTimer.Start();
        }

        /// <inheritdoc />
        public event EventHandler<DeviceContext> ContextCreated;

        /// <inheritdoc />
        public event EventHandler<DeviceContext> Render;

        /// <inheritdoc />
        public event EventHandler<DeviceContext> ContextUpdate;

        /// <inheritdoc />
        public event EventHandler<DeviceContext> ContextDestroying;

        /// <inheritdoc />
        public event EventHandler<char> KeyDown;

        /// <inheritdoc />
        public event EventHandler<char> KeyUp;

        /// <inheritdoc />
        public event EventHandler<int> MouseWheel;

        /// <inheritdoc />
        public event EventHandler MouseUp;

        /// <inheritdoc />
        public event EventHandler<Vector2> Resize;

        /// <inheritdoc />
        public event EventHandler Update;

        /// <inheritdoc />
        public event EventHandler GotFocus;

        /// <inheritdoc />
        int IViewContext.Width => glControl.ClientSize.Width;

        /// <inheritdoc />
        int IViewContext.Height => glControl.ClientSize.Height;

        /// <inheritdoc />
        Vector2 IViewContext.CursorPosition
        {
            get
            {
                var point = glControl.PointToClient(Cursor.Position);
                return new Vector2(point.X, point.Y);
            }
            set
            {
                var point = new Point((int)value.X, (int)value.Y);
                Cursor.Position = glControl.PointToScreen(point);
            }
        }

        /// <inheritdoc />
        public bool IsFocused => glControl.Focused;

        /// <inheritdoc />
        public void HideCursor()
        {
            Cursor.Hide();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            modelUpdateTimer?.Dispose();
        }
    }
}
