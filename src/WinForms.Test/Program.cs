namespace WinForms.Test
{
    using OpenGlHelpers.Core;
    using OpenGlHelpers.WinForms;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;
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

            camera = new FirstPersonCamera();
            var renderer = new StaticTexuredRenderer(
                new[] { new Vector3(-1f, -1f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1f, -1f, 0f) },
                new[] { new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, -1f) },
                new[] { new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(1f, 0f) },
                new[] { 0u, 1u, 2u },
                camera);
            form = new OpenGlForm(renderer, ModelUpdate, true);
            Application.Run(form);
        }

        private static void ModelUpdate(TimeSpan elapsed)
        {
            camera.Update(elapsed, form);
        }
    }
}
