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

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            form = new OpenGlForm(
                new[]
                {
                    new StaticTexuredRenderer(
                        new[] { new Vector3(-1f, -1f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1f, -1f, 0f) },
                        new[] { new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, -1f) },
                        new[] { new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(1f, 0f) },
                        new[] { 0u, 1u, 2u })
                },
                new FirstPersonCamera(),
                ModelUpdate,
                true);
            Application.Run(form);
        }

        private static void ModelUpdate(TimeSpan elapsed)
        {
        }
    }
}
