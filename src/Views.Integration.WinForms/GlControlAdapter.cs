namespace GLHDN.Views.Integration.WinForms
{
    using GLHDN.Views;
    using OpenGL;
    using System;
    using System.Drawing;
    using System.Numerics;
    using System.Windows.Forms;

    public sealed class GlControlAdapter : IViewContext, IDisposable
    {
        private GlControl glControl;
        private readonly Timer modelUpdateTimer; // TODO: this is the wrong timer type to use - it's tied to forms update

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

            this.modelUpdateTimer = new Timer();
            this.modelUpdateTimer.Interval = 15;
            this.modelUpdateTimer.Tick += (s, a) => Update(this, EventArgs.Empty);
            this.modelUpdateTimer.Start();
        }

        public event EventHandler<DeviceContext> ContextCreated;
        public event EventHandler<DeviceContext> Render;
        public event EventHandler<DeviceContext> ContextUpdate;
        public event EventHandler<DeviceContext> ContextDestroying;
        public event EventHandler<char> KeyDown;
        public event EventHandler<char> KeyUp;
        public event EventHandler<int> MouseWheel;
        public event EventHandler MouseUp;
        public event EventHandler<Vector2> Resize;
        public event EventHandler Update;
        public event EventHandler GotFocus;

        int IViewContext.Width => glControl.ClientSize.Width;

        int IViewContext.Height => glControl.ClientSize.Height;

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

        public bool IsFocused => glControl.Focused;

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
