namespace GLHDN.Views.Contexts.WinForms
{
    using OpenGL;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// Windows form containing only a single OpenGL render control.
    /// </summary>
    public sealed class GlForm : Form
    {
        private readonly GlControl glControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlForm"/> class.
        /// </summary>
        public GlForm()
        {
            this.SuspendLayout();

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            //this.Size = new Size(1000, 600);
            this.Name = "OpenGl";
            this.Text = "OpenGl";

            this.glControl = new GlControl()
            {
                Animation = true,
                BackColor = Color.DimGray,
                ColorBits = 24u,
                DepthBits = 8u,
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                Margin = new Padding(0, 0, 0, 0),
                MultisampleBits = 0u,
                Name = "RenderControl",
                StencilBits = 0u,
                TabIndex = 0
            };

            this.Controls.Add(glControl);
            this.ResumeLayout(false);

            this.ViewContext = new GlControlAdapter(glControl);
        }

        /// <summary>
        /// Gets the <see cref="IViewContext" /> of this form for <see cref="View"/> instances to use.
        /// </summary>
        public GlControlAdapter ViewContext { get; }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                glControl?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}