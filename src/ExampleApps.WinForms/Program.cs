namespace GLHDN.Examples.WinForms
{
    using GLHDN.Views;
    using GLHDN.Views.Contexts.WinForms;
    using GLHDN.Views.Renderables.BasicExamples;
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

            var form = new GlForm()
            {
                // WindowState = FormWindowState.Normal,
                // FormBorderStyle = FormBorderStyle.Sizable
            };

            view = new Views.View(form.ViewContext, ModelUpdate, true, Vector3.Zero);

            camera = new FirstPersonCamera(
                movementSpeed: 3.0f,
                rotationSpeed: 0.005f,
                fieldOfViewRadians: (float)Math.PI / 4.0f,
                nearPlaneDistance: 0.1f,
                farPlaneDistance: 100f,
                initialPosition: new Vector3(0f, 0f, -3f),
                initialHorizontalAngleRadians: 0f,
                initialVerticalAngleRadians: 0f);

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

            view.Renderables.Add(Program.lines = new ColoredLines(camera));

            gui = new Gui(view);
            gui.Initialized += (s, e) =>
            {
                var panel = new PanelElement(
                    parentOrigin: new Dimensions(-1f, 0f),
                    localOrigin: new Dimensions(-1f, 0f),
                    relativeSize: new Dimensions(250, 1f),
                    color: new Vector4(1f, 1f, 1f, 0f),
                    borderWidth: 0f);
                gui.SubElements.Add(panel);

                panel.SubElements.Add(camText = new TextElement(
                    parentOrigin: new Dimensions(-1f, 1f),
                    localOrigin: new Dimensions(-1f, 1f),
                    relativeSize: new Dimensions(1f, 0f),
                    color: new Vector4(1f, 1f, 1f, 1f)));
            };

            Application.Run(form);
        }

        private static void ModelUpdate(TimeSpan elapsed)
        {
            camera.Update(elapsed, view);
            camText.Content = $"Cam: {camera.Position:F2}\n\nHello, world!";

            if (view.WasLeftMouseButtonReleased)
            {
                var ray = new Ray(camera, view);
                lines.AddLine(ray.Origin, ray.Origin + ray.Direction * 10);
            }
        }
    }
}
