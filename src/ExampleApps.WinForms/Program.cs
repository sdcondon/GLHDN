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

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var form = new GlForm("GLHDN Example App")
            {
                // WindowState = FormWindowState.Normal,
                // FormBorderStyle = FormBorderStyle.Sizable
            };

            TextElement.Font = new Font(@"Assets\Fonts\Inconsolata\Inconsolata-Regular.ttf");

            var view = new View(form.ViewContext, false, Color.Black());
            view.Renderable = new MenuRenderable(view);

            System.Windows.Forms.Application.Run(form);
        }

        private class MenuRenderable : CompositeRenderable
        {
            public MenuRenderable(View view)
            {
                AddRenderable(new Gui(view)
                {
                    SubElements =
                    {
                        new Button(
                            layout: new Layout((0f, 0f), (0f, 0f), (200, 40), new Vector2(0, 50)),
                            color: Color.Blue(.5f),
                            textColor: Color.White(),
                            text: "DEMO",
                            v =>
                            {
                                view.Renderable = new DemoRenderable(view);
                                this.Dispose();
                            }),
                        new Button(
                            layout: new Layout((0f, 0f), (0f, 0f), (200, 40), new Vector2(0, -10)),
                            color: Color.Blue(.5f),
                            textColor: Color.White(),
                            text: "QUIT",
                            v => view.Exit()),
                        new TextElement(
                            layout: new Layout((0f, -1f), (0f, -1f), (1f, 40)),
                            color: Color.Grey(0.7f),
                            content: "Here is a footer. Hello!")
                        {
                            HorizontalAlignment = 0.5f
                        }
                    },
                });
            }
        }

        private class DemoRenderable : CompositeRenderable
        {
            private readonly View view;
            private readonly ICamera camera;

            private readonly ColoredLines lines;
            private readonly TextElement camTextElement;
            private readonly TextStreamElement logElement;

            private readonly Subject<IList<Primitive>> cubeSubject = new Subject<IList<Primitive>>();
            private Matrix4x4 cubeWorldMatrix = Matrix4x4.Identity;

            public DemoRenderable(View view)
            {
                this.view = view;
                camera = new FirstPersonCamera(
                    view,
                    movementSpeed: 3.0f,
                    rotationSpeed: 0.005f,
                    fieldOfViewRadians: (float)Math.PI / 4.0f,
                    nearPlaneDistance: 0.1f,
                    farPlaneDistance: 100f,
                    initialPosition: new Vector3(0f, 0f, 3f),
                    initialHorizontalAngleRadians: (float)Math.PI,
                    initialVerticalAngleRadians: 0f);

                AddRenderable(new TexturedStaticMesh(
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
                    @"Assets\Textures\foo.bmp"));

                AddRenderable(new PrimitiveRenderer(camera, Observable.Return(cubeSubject), 12)
                {
                    AmbientLightColor = Color.Grey(0.1f),
                    DirectedLightDirection = new Vector3(0, 1f, 0f),
                    DirectedLightColor = Color.Grey()
                });

                AddRenderable(lines = new ColoredLines(camera));

                camTextElement = new TextElement(
                    new Layout((-1f, 1f), (-1f, 1f), (1f, 0f)),
                    color: Color.White());

                logElement = new TextStreamElement(
                    new Layout((-1f, 1f), (-1f, 1f), (1f, 0f), new Vector2(0, -100)),
                    textColor: Color.White(),
                    10);

                AddRenderable(new Gui(view)
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
                                logElement
                            },
                        },
                    },
                });
            }

            public override void Update(TimeSpan elapsed)
            {
                base.Update(elapsed);

                camera.Update(elapsed);

                camTextElement.Content = $"Hello, world!\n\nCam: {camera.Position:F2}\n\nPress SPACE to toggle cam mode\nPress q to quit";

                cubeWorldMatrix *= Matrix4x4.CreateRotationZ((float)elapsed.TotalSeconds);
                cubeWorldMatrix *= Matrix4x4.CreateRotationY((float)elapsed.TotalSeconds / 2);
                cubeSubject.OnNext(new[] { Primitive.Cuboid(new Vector3(.5f, 1f, 0.75f), cubeWorldMatrix, Color.Red()) }); // TODO: no new array each time

                if (view.WasLeftMouseButtonReleased)
                {
                    var ray = new Ray(camera, view);
                    lines.AddLine(ray.Origin, ray.Origin + ray.Direction * 10);
                    logElement.PushMessage($"RAY FROM {ray.Origin:F2}");
                }

                if (view.KeysReleased.Contains(' '))
                {
                    view.LockCursor = !view.LockCursor;
                }

                if (view.KeysReleased.Contains('Q'))
                {
                    view.Renderable = new MenuRenderable(view);
                    this.Dispose();
                }
            }
        }
    }
}
