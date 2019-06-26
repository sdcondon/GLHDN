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
    using WinFormsApp = System.Windows.Forms.Application;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            WinFormsApp.EnableVisualStyles();
            WinFormsApp.SetCompatibleTextRenderingDefault(false);

            var form = new GlForm()
            {
                // WindowState = FormWindowState.Normal,
                // FormBorderStyle = FormBorderStyle.Sizable
            };

            var view = new View(form.ViewContext, true, Color.Black());

            var demo = new DemoRenderable(view);
            view.Renderables.Add(demo);
            view.Update += demo.Update;

            WinFormsApp.Run(form);
        }

        private class DemoRenderable : CompositeRenderable
        {
            private readonly ICamera camera;

            private readonly ColoredLines lines;
            private readonly Gui gui;
            private readonly TextElement camTextElement;

            private Matrix4x4 cubeWorldMatrix = Matrix4x4.Identity;
            private Subject<IList<Primitive>> cubeSubject = new Subject<IList<Primitive>>();

            public DemoRenderable(Views.View view)
            {
                camera = new FirstPersonCamera(
                    movementSpeed: 3.0f,
                    rotationSpeed: 0.005f,
                    fieldOfViewRadians: (float)Math.PI / 4.0f,
                    nearPlaneDistance: 0.1f,
                    farPlaneDistance: 100f,
                    initialPosition: new Vector3(0f, 0f, 3f),
                    initialHorizontalAngleRadians: (float)Math.PI,
                    initialVerticalAngleRadians: 0f);

                Renderables.Add(new TexturedStaticMesh(
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

                Renderables.Add(new PrimitiveRenderer(camera, Observable.Return(cubeSubject))
                {
                    AmbientLightColor = Color.Grey(0.1f),
                    DirectedLightDirection = new Vector3(0, 1f, 0f),
                    DirectedLightColor = Color.Grey()
                });

                Renderables.Add(lines = new ColoredLines(camera));

                camTextElement = new TextElement(
                    new Layout((-1f, 1f), (-1f, 1f), (1f, 0f)),
                    color: Color.White());
                Renderables.Add(gui = new Gui(view)
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
                                camTextElement,
                            },
                        },
                    },
                });
            }

            public void Update(object sender, TimeSpan elapsed)
            {
                var view = (View)sender;

                camera.Update(elapsed, view);
                gui.Update(); // todo: Iupdateable?
                camTextElement.Content = $"Hello, world!\n\nCam: {camera.Position:F2}\n\nPress q to quit";

                cubeWorldMatrix *= Matrix4x4.CreateRotationZ((float)elapsed.TotalSeconds);
                cubeWorldMatrix *= Matrix4x4.CreateRotationY((float)elapsed.TotalSeconds / 2);
                cubeSubject.OnNext(new[] { Primitive.Cuboid(new Vector3(.5f, 1f, 0.75f), cubeWorldMatrix, Color.Red()) }); // todo: no new array each time

                if (view.WasLeftMouseButtonReleased)
                {
                    var ray = new Ray(camera, view);
                    lines.AddLine(ray.Origin, ray.Origin + ray.Direction * 10);
                }

                if (view.KeysReleased.Contains(' '))
                {
                    view.LockCursor = !view.LockCursor;
                }

                // todo: q to quit
                // todo: iviewcontext.exit & view.exit
                // todo: menu to move to demo & to quit
            }
        }
    }
}
