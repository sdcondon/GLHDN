namespace GLHDN.Views.Contexts.WinForms
{
    using OpenGL;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// Windows form containing only a single OpenGL render control, with handlers as passed to the forms constructor.
    /// </summary>
    public sealed class GlForm : Form
    {
        public GlForm()
        {
            this.SuspendLayout();

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.Name = "OpenGl";
            this.Text = "OpenGl";

            var glControl = new GlControl()
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

        public GlControlAdapter ViewContext { get; private set; }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ViewContext?.Dispose();
                ViewContext = null;
            }

            base.Dispose(disposing);
        }
    }
}