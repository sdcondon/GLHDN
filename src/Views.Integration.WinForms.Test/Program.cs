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
        private static Views.View view;

        private static ColoredLines lines;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new GlForm();

            view = new Views.View(form.ViewContext, ModelUpdate, true);

            camera = new FirstPersonCamera();

            view.Renderables.Add(new StaticTexuredRenderer(
                camera,
                new[] { new Vector3(-1f, -1f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1f, -1f, 0f) },
                new[] { new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, -1f) },
                new[] { new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(1f, 0f) },
                new[] { 0u, 1u, 2u },
                "uvmap.DDS"));

            lines = new ColoredLines(camera);
            view.Renderables.Add(lines);

            gui = new Gui(
                view,
                new[]
                {
                    new GuiElement(null)
                    {
                        ParentOrigin = new Vector2(-1, 0),
                        IsParentOriginXRelative = true,
                        IsParentOriginYRelative = true,
                        LocalOrigin = new Vector2(-1, 0),
                        IsLocalOriginXRelative = true,
                        IsLocalOriginYRelative = true,
                        Size = new Vector2(200f, 1f),
                        IsSizeXRelative = false,
                        IsSizeYRelative = true,
                        Color = new Vector4(0.2f, 0.2f, 0.2f, 0.2f),
                        BorderWidth = 3f
                    }
                });

            Application.Run(form);
        }

        private static void ModelUpdate(TimeSpan elapsed)
        {
            camera.Update(elapsed, view);
            gui.Update();

            if (view.MouseButtonReleased)
            {
                var ray = new Ray(camera, view);
                lines.AddLine(ray.Origin, ray.Origin + ray.Direction * 10);
            }
        }
    }
}
