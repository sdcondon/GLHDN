namespace WinForms.Test
{
    using OpenGlHelpers.Core;
    using OpenGlHelpers.WinForms;
    using System;
    using System.Numerics;
    using System.Windows.Forms;

    static class Program
    {
        private static ICamera camera;
        private static OpenGlForm form;

        private static ColoredLines lines;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            camera = new FirstPersonCamera();
            lines = new ColoredLines(camera);
            var gui = new Gui();
            gui.AddElement(new GuiElement() { TopLeft = new Vector2(0f, 0f), Width = 1, Height = 1, Color = Vector3.One });

            form = new OpenGlForm(
                new IRenderable[]
                {
                    new StaticTexuredRenderer(
                        camera,
                        new[] { new Vector3(-1f, -1f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1f, -1f, 0f) },
                        new[] { new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, -1f) },
                        new[] { new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(1f, 0f) },
                        new[] { 0u, 1u, 2u }),
                    lines,
                    gui
                },
                ModelUpdate,
                true);
            Application.Run(form);
        }

        private static void ModelUpdate(TimeSpan elapsed)
        {
            camera.Update(elapsed, form);

            if (form.MouseButtonReleased)
            {
                var ray = new Ray(camera, form);
                lines.AddLine(ray.Origin, ray.Origin + ray.Direction * 10);
            }
        }
    }
}
