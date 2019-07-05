namespace GLHDN.Views.Contexts.WinForms
{
    using OpenGL;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// Windows form containing only a single OpenGL render control.
    /// </summary>
    [DesignerCategory("")]
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
        /// Gets or sets the flag indicating whether control should update itself continuously.
        /// </summary>
        public bool Animation
        {
            get => glControl.Animation;
            set => glControl.Animation = value;
        }

        /// <summary>
        /// Gets or sets the time interval between animation updates, in milliseconds.
        /// </summary>
        public int AnimationTime
        {
            get => glControl.AnimationTime;
            set => glControl.AnimationTime = value;
        }

        /// <summary>
        /// Gets or sets the OpenGL minimum color buffer bits.
        /// </summary>
        public uint ColorBits
        {
            get => glControl.ColorBits;
            set => glControl.ColorBits = value;
        }

        /// <summary>
        /// Gets or sets the OpenGL minimum depth buffer bits.
        /// </summary>
        public uint DepthBits
        {
            get => glControl.DepthBits;
            set => glControl.DepthBits = value;
        }

        /// <summary>
        /// Gets or sets the OpenGL minimum multisample buffer "bits".
        /// </summary>
        public uint MultisampleBits
        {
            get => glControl.MultisampleBits;
            set => glControl.MultisampleBits = value;
        }

        /// <summary>
        /// Gets or sets the OpenGL minimum stencil buffer bits.
        /// </summary>
        public uint StencilBits
        {
            get => glControl.StencilBits;
            set => glControl.StencilBits = value;
        }

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