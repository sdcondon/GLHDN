namespace GLHDN.Views.Contexts.WinForms.Test
{
    using GLHDN.Views.Renderables.Basic;
    using GLHDN.Views.Renderables.Gui;
    using System;
    using System.Numerics;
    using System.Windows.Forms;

    static class Program
    {
        private static ICamera camera;
        private static Gui gui;
        private static Views.View view;

        private static ColoredLines lines;
        private static TextElement camText;

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
                new[]
                {
                    new StaticTexuredRenderer.Vertex(
                        new Vector3(-1f, -1f, 0f),
                        new Vector2(0f, 0f),
                        new Vector3(0f, 0f, -1f)),
                    new StaticTexuredRenderer.Vertex(
                        new Vector3(0f, 1f, 0f),
                        new Vector2(0.5f, 1f),
                        new Vector3(0f, 0f, -1f)),
                    new StaticTexuredRenderer.Vertex(
                        new Vector3(1f, -1f, 0f),
                        new Vector2(1f, 0f),
                        new Vector3(0f, 0f, -1f))
                },
                new uint[] { 0, 1, 2 },
                "uvmap.DDS"));

            lines = new ColoredLines(camera);
            view.Renderables.Add(lines);

            gui = new Gui(view);
            gui.Initialized += (s, e) =>
            {
                var panel = new PanelElement()
                {
                    ParentOrigin = new Dimensions(-1f, 0f),
                    LocalOrigin = new Dimensions(-1f, 0f),
                    RelativeSize = new Dimensions(200, 1f),
                    Color = new Vector4(0.2f, 0.2f, 0.5f, 0.5f),
                    BorderWidth = 0f
                };
                gui.SubElements.Add(panel);
                camText = new TextElement($"X: {camera.Position.X:F2}")
                {
                    ParentOrigin = new Dimensions(-1f, 1f),
                    LocalOrigin = new Dimensions(-1f, 1f),
                    RelativeSize = new Dimensions(1f, 0f),
                    Color = new Vector4(1f, 1f, 1f, 1f)
                };
                panel.SubElements.Add(camText);
            };

            Application.Run(form);
        }

        private static void ModelUpdate(TimeSpan elapsed)
        {
            camera.Update(elapsed, view);
            gui.Update();
            camText.Content = $"Cam: {camera.Position:F2}\n\nHello, world!";

            if (view.MouseButtonReleased)
            {
                var ray = new Ray(camera, view);
                lines.AddLine(ray.Origin, ray.Origin + ray.Direction * 10);
            }
        }
    }
}
