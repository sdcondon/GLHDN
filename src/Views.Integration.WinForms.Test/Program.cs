namespace GLHDN.Views.Integration.WinForms.Test
{
    using GLHDN.Views;
    using GLHDN.Views.Integration.WinForms;
    using System;
    using System.Numerics;
    using System.Windows.Forms;

    static class Program
    {
        private static ICamera camera;
        private static Gui gui;
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
            gui = new Gui(new[]
            {
                new GuiElement(null)
                {
                    //ParentOrigin = new Vector2(-1, 0),
                    //IsParentOriginXRelative = true,
                    //IsParentOriginYRelative = true,
                    //LocalOrigin = new Vector2(-1, 0),
                    //IsLocalOriginXRelative = true,
                    //IsLocalOriginYRelative = true,
                    Size = new Vector2(200f, 1f),
                    IsSizeXRelative = false,
                    IsSizeYRelative = true,
                    Color = new Vector4(0.2f, 0.2f, 0.2f, 1f),
                    BorderWidth = 3f
                }
            });

            var view = new Views.View(
                new StaticTexuredRenderer(
                    camera,
                    new[] { new Vector3(-1f, -1f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1f, -1f, 0f) },
                    new[] { new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, -1f) },
                    new[] { new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(1f, 0f) },
                    new[] { 0u, 1u, 2u },
                    "uvmap.DDS"),
                lines,
                gui);
            //view.Renderables.Add();

            form = new OpenGlForm(view, ModelUpdate, true);
            Application.Run(form);
        }

        private static void ModelUpdate(TimeSpan elapsed)
        {
            camera.Update(elapsed, form);
            gui.Update(form);

            if (form.MouseButtonReleased)
            {
                var ray = new Ray(camera, form);
                lines.AddLine(ray.Origin, ray.Origin + ray.Direction * 10);
            }
        }
    }
}
