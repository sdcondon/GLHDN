namespace GLHDN.Examples.WinForms
{
    using GLHDN.Views;
    using GLHDN.Views.Contexts.WinForms;
    using GLHDN.Views.Renderables.BasicExamples;
    using GLHDN.Views.Renderables.Gui;
    using GLHDN.Views.Renderables.Primitives;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Windows.Forms;

    static class Program
    {
        private static Views.View view;
        private static ICamera camera;

        private static ColoredLines lines;
        private static Gui gui;
        private static TextElement camText;

        private static Matrix4x4 cubeWorldMatrix = Matrix4x4.Identity;
        private static Subject<IList<Primitive>> cubeSubject = new Subject<IList<Primitive>>();

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
                WindowState = FormWindowState.Normal,
                FormBorderStyle = FormBorderStyle.Sizable
            };

            view = new Views.View(form.ViewContext, true, Vector3.Zero);
            view.Update += ModelUpdate;

            camera = new FirstPersonCamera(
                movementSpeed: 3.0f,
                rotationSpeed: 0.005f,
                fieldOfViewRadians: (float)Math.PI / 4.0f,
                nearPlaneDistance: 0.1f,
                farPlaneDistance: 100f,
                initialPosition: new Vector3(0f, 0f, 3f),
                initialHorizontalAngleRadians: (float)Math.PI,
                initialVerticalAngleRadians: 0f);

            view.Renderables.Add(new TexturedStaticMesh(
                camera,
                new[]
                {
                    new TexturedStaticMesh.Vertex(
                        new Vector3(-1f, -1f, -2f),
                        new Vector2(0f, 0f),
                        new Vector3(0f, 0f, 1f)),
                    new TexturedStaticMesh.Vertex(
                        new Vector3(1f, -1f, -2f),
                        new Vector2(1f, 0f),
                        new Vector3(0f, 0f, 1f)),
                    new TexturedStaticMesh.Vertex(
                        new Vector3(0f, 1f, -2f),
                        new Vector2(0.5f, 1f),
                        new Vector3(0f, 0f, 1f)),
                },
                new uint[] { 0, 1, 2 },
                "uvmap.DDS"));

            view.Renderables.Add(new PrimitiveRenderer(camera, Observable.Return(cubeSubject))
            {
                AmbientLightColor = Color.Grey(0.1f),
                DirectedLightDirection = new Vector3(0, 1f, 0f),
                DirectedLightColor = Color.Grey()
            });

            view.Renderables.Add(Program.lines = new ColoredLines(camera));

            Program.camText = new TextElement(
                new Layout((-1f, 1f), (-1f, 1f), (1f, 0f)),
                color: Color.White());
            view.Renderables.Add(gui = new Gui(view)
            {
                SubElements =
                {
                    new PanelElement(
                        layout: new Layout((-1f, 0f), (-1f, 0f), (250, 1f)),
                        color: Color.White(0.05f),
                        borderWidth: 0f)
                    {
                        SubElements =
                        {
                            camText,
                            new Views.Renderables.Gui.Button(
                                layout: new Layout((1f, 1f), (1f, 1f), (100, 100)),
                                color: Color.Blue(),
                                textColor: Color.White(),
                                text: "Dispose",
                                (s, e) => view.Dispose())
                        }
                    }
                }
            });

            Application.Run(form);
        }

        private static void ModelUpdate(object sender, TimeSpan elapsed)
        {
            camera.Update(elapsed, view);
            gui.Update();
            camText.Content = $"Hello, world!\n\nCam: {camera.Position:F2}";

            cubeWorldMatrix *= Matrix4x4.CreateRotationZ((float)elapsed.TotalSeconds);
            cubeWorldMatrix *= Matrix4x4.CreateRotationY((float)elapsed.TotalSeconds / 2);
            cubeSubject.OnNext(new[] { Primitive.Cuboid(new Vector3(.5f, 1f, 0.75f), cubeWorldMatrix, Color.Red()) });

            if (view.WasLeftMouseButtonReleased)
            {
                var ray = new Ray(camera, view);
                lines.AddLine(ray.Origin, ray.Origin + ray.Direction * 10);
            }
        }
    }
}
