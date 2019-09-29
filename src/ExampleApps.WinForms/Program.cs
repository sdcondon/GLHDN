namespace GLHDN.Examples.WinForms
{
    using GLHDN.Views;
    using GLHDN.Views.Contexts.WinForms;
    using GLHDN.Views.Renderables.BasicExamples;
    using GLHDN.Views.Renderables.Gui;
    using GLHDN.Views.Renderables.Primitives;
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Text;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            var form = new GlForm("GLHDN Example App")
            {
                // WindowState = FormWindowState.Normal,
                // FormBorderStyle = FormBorderStyle.Sizable
            };

            // Obviously not ideal to have to set font globally - need to sort this out, probably via some nice
            // texture management stuff in the core lib.
            TextElement.Font = new Font(@"Assets\Fonts\Inconsolata\Inconsolata-Regular.ttf");

            // A View encapsulates an interactive OpenGl rendered viewport
            var view = new View(form.ViewContext, false, Color.Black());

            // Views have a Renderable property. Renderables can be composed of other renderables -
            // there's a handy CompositeRenderable base class to make this easy - see the two examples
            // below, and note that MenuRenderable (which consists of a single Gui renderable and has
            // no overrides) is only a class of its own to make disposal on button press easy.
            view.Renderable = new MenuRenderable(view);

            System.Windows.Forms.Application.Run(form);
        }

        private class MenuRenderable : CompositeRenderable
        {
            public MenuRenderable(View view)
            {
                view.LockCursor = false;

                var systemInfo = new StringBuilder();
                systemInfo.AppendLine($"OpenGl version: {Gl.CurrentVersion}");
                systemInfo.AppendLine($"OpenGl Shading Language version: {Gl.CurrentShadingVersion}");
                systemInfo.AppendLine($"Vendor: {Gl.CurrentVendor}");
                systemInfo.AppendLine($"Renderer: {Gl.CurrentRenderer}");
                if (Egl.IsAvailable)
                {
                    systemInfo.AppendLine("EGL is available");
                }

                AddRenderable(new Gui(view, 1000)
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
                            layout: new Layout((0f, -1f), (0f, -1f), (1f, 100)),
                            color: Color.Grey(0.7f),
                            content: systemInfo.ToString())
                        {
                            HorizontalAlignment = 0.5f,
                        },
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
            private readonly Primitive[] cubePrimitives = new Primitive[1] { Primitive.Empty() };
            private Matrix4x4 cubeWorldMatrix = Matrix4x4.Identity;
            private Vector3 lastCamPosition = Vector3.Zero;

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

                var texturedTriangleVertices = new[]
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
                };
                AddRenderable(new TexturedStaticMesh(
                    camera,
                    texturedTriangleVertices,
                    new uint[] { 0, 1, 2 },
                    @"Assets\Textures\foo.bmp")
                {
                    AmbientLightColor = Color.Grey(0.3f),
                });

                var coloredTriangleVertices = new[]
                {
                    new ColoredStaticMesh.Vertex(
                        new Vector3(1f, 1f, -3f),
                        Color.Orange(),
                        new Vector3(0f, 0f, 1f)),
                    new ColoredStaticMesh.Vertex(
                        new Vector3(-1f, 1f, -3f),
                        Color.Orange(),
                        new Vector3(0f, 0f, 1f)),
                    new ColoredStaticMesh.Vertex(
                        new Vector3(0f, -1f, -3f),
                        Color.Orange(),
                        new Vector3(0f, 0f, 1f)),
                };
                AddRenderable(new ColoredStaticMesh(
                    camera,
                    coloredTriangleVertices,
                    new uint[] { 0, 1, 2 })
                {
                    AmbientLightColor = Color.Grey(0.3f),
                });

                AddRenderable(new PrimitiveRenderer(camera, Observable.Return(cubeSubject), 12)
                {
                    AmbientLightColor = Color.Grey(0.1f),
                    DirectedLightDirection = new Vector3(0, 1f, 0f),
                    DirectedLightColor = Color.Grey(),
                });

                AddRenderable(lines = new ColoredLines(camera)
                {
                    AmbientLightColor = Color.Grey(0.3f),
                });

                camTextElement = new TextElement(
                    new Layout((-1f, 1f), (-1f, 1f), (1f, 0f)),
                    color: Color.White());

                logElement = new TextStreamElement(
                    new Layout((-1f, 1f), (-1f, 1f), (1f, 0f), new Vector2(0, -100)),
                    textColor: Color.White(),
                    10);

                AddRenderable(new Gui(view, 1000)
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
                                logElement,
                            },
                        },
                    },
                });
            }

            public override void Update(TimeSpan elapsed)
            {
                base.Update(elapsed);

                camera.Update(elapsed);

                // Avoid GC pressure for string unless needed - would be better to do this reactively though
                // (e.g. reactive linq to take at intervals or debounce)
                if (camera.Position != lastCamPosition)
                {
                    camTextElement.Content = $"Cam@{camera.Position:F2}\n\nPress SPACE to toggle cam mode\nPress q to quit";
                    lastCamPosition = camera.Position;
                }

                // NB: No new heap allocations each time to avoid GC pressure - same array, same primitive.
                // Could do with more helpers to make this easier. Perhaps Primitive should be a struct after all..
                cubeWorldMatrix *= Matrix4x4.CreateRotationZ((float)elapsed.TotalSeconds);
                cubeWorldMatrix *= Matrix4x4.CreateRotationY((float)elapsed.TotalSeconds / 2);
                cubePrimitives[0].SetCuboid(new Vector3(.5f, 1f, 0.75f), cubeWorldMatrix, Color.Red());
                cubeSubject.OnNext(cubePrimitives);

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
