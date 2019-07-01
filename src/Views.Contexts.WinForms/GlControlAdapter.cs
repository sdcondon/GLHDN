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
    public sealed class GlControlAdapter : IViewContext
    {
        private GlControl glControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlControlAdapter"/> class.
        /// </summary>
        /// <param name="glControl">The <see cref="GlControl"/> to wrap.</param>
        public GlControlAdapter(GlControl glControl)
        {
            this.glControl = glControl;
            this.glControl.ContextCreated += (s, a) => GlContextCreated?.Invoke(this, a.DeviceContext);
            this.glControl.Render += (s, a) => GlRender?.Invoke(this, a.DeviceContext);
            this.glControl.ContextUpdate += (s, a) => GlContextUpdate?.Invoke(this, a.DeviceContext);
            this.glControl.ContextDestroying += (s, a) => GlContextDestroying?.Invoke(this, a.DeviceContext);
            this.glControl.KeyDown += (s, a) => KeyDown?.Invoke(this, (char)a.KeyValue);
            this.glControl.KeyUp += (s, a) => KeyUp?.Invoke(this, (char)a.KeyValue);
            this.glControl.MouseWheel += (s, a) => MouseWheel?.Invoke(this, a.Delta);
            this.glControl.MouseDown += (s, a) =>
            {
                switch (a.Button)
                {
                    case MouseButtons.Left:
                        LeftMouseDown?.Invoke(this, EventArgs.Empty);
                        break;
                    case MouseButtons.Right:
                        RightMouseDown?.Invoke(this, EventArgs.Empty);
                        break;
                    case MouseButtons.Middle:
                        MiddleMouseDown?.Invoke(this, EventArgs.Empty);
                        break;
                }
            };
            this.glControl.MouseUp += (s, a) =>
            {
                switch (a.Button)
                {
                    case MouseButtons.Left:
                        LeftMouseUp?.Invoke(this, EventArgs.Empty);
                        break;
                    case MouseButtons.Right:
                        RightMouseUp?.Invoke(this, EventArgs.Empty);
                        break;
                    case MouseButtons.Middle:
                        MiddleMouseUp?.Invoke(this, EventArgs.Empty);
                        break;
                }
            };
            this.glControl.Resize += (s, a) => Resize?.Invoke(this, new Vector2(this.glControl.ClientSize.Width, this.glControl.ClientSize.Height));
            this.glControl.GotFocus += (s, a) => GotFocus?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public event EventHandler<DeviceContext> GlContextCreated;

        /// <inheritdoc />
        public event EventHandler<DeviceContext> GlRender;

        /// <inheritdoc />
        public event EventHandler<DeviceContext> GlContextUpdate;

        /// <inheritdoc />
        public event EventHandler<DeviceContext> GlContextDestroying;

        /// <inheritdoc />
        public event EventHandler<char> KeyDown;

        /// <inheritdoc />
        public event EventHandler<char> KeyUp;

        /// <inheritdoc />
        public event EventHandler<int> MouseWheel;

        /// <inheritdoc />
        public event EventHandler LeftMouseDown;

        /// <inheritdoc />
        public event EventHandler LeftMouseUp;

        /// <inheritdoc />
        public event EventHandler RightMouseDown;

        /// <inheritdoc />
        public event EventHandler RightMouseUp;

        /// <inheritdoc />
        public event EventHandler MiddleMouseDown;

        /// <inheritdoc />
        public event EventHandler MiddleMouseUp;

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
        public bool IsFocused => glControl.Focused;

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
        public bool ShowCursor
        {
            set
            {
                if (value)
                {
                    Cursor.Show();
                }
                else
                {
                    Cursor.Hide();
                }
            }
        }

        /// <inheritdoc />
        public void Exit()
        {
            Application.Exit();
        }
    }
}
