namespace OpenGlHelpers.WinForms
{
    using OpenGlHelpers.Core;
    using System;
    using System.Numerics;

    public class OrbitCamera : ICamera
    {
        private const float rotationSpeed = 0.005f;

        private float distance = 2.5f;
        private Vector3 forward = new Vector3(0f, 0f, 1f);
        private Vector3 up = new Vector3(0f, 1f, 0f);

        // Field of View, in radians
        public float FieldOfView { get; set; } = (float)Math.PI / 4.0f;

        /// <inheritdoc />
        public Vector3 Position => -forward * distance;

        /// <inheritdoc />
        public Matrix4x4 ViewMatrix
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public Matrix4x4 ProjectionMatrix
        {
            get;
            private set;
        }

        public void Update(TimeSpan elapsed, IUiContext context)
        {
            // Pan up - rotate forward and up around their cross product
            if (context.PressedKeys.Contains('W'))
            {
                var t = Matrix4x4.CreateFromAxisAngle(Vector3.Cross(forward, up), -rotationSpeed);
                forward = Vector3.Transform(forward, t);
                up = Vector3.Transform(up, t);
            }
            // Pan down - rotate forward and up around their cross product
            if (context.PressedKeys.Contains('S'))
            {
                var t = Matrix4x4.CreateFromAxisAngle(Vector3.Cross(forward, up), rotationSpeed);
                forward = Vector3.Normalize(Vector3.Transform(forward, t));
                up = Vector3.Normalize(Vector3.Transform(up, t));
            }
            // Pan right - rotate forward around up
            if (context.PressedKeys.Contains('D'))
            {
                forward = Vector3.Normalize(Vector3.Transform(forward, Matrix4x4.CreateFromAxisAngle(up, rotationSpeed)));
            }
            // Pan left - rotate forward around up
            if (context.PressedKeys.Contains('A'))
            {
                forward = Vector3.Normalize(Vector3.Transform(forward, Matrix4x4.CreateFromAxisAngle(up, -rotationSpeed)));
            }
            // Roll right - rotate up around forward
            if (context.PressedKeys.Contains('Q'))
            {
                up = Vector3.Normalize(Vector3.Transform(up, Matrix4x4.CreateFromAxisAngle(forward, -rotationSpeed)));
            }
            // Roll left - rotate up around forward
            if (context.PressedKeys.Contains('E'))
            {
                up = Vector3.Normalize(Vector3.Transform(up, Matrix4x4.CreateFromAxisAngle(forward, rotationSpeed)));
            }

            // Projection matrix : 45° Field of View, 4:3 ratio, display range : 0.1 unit <-> 100 units
            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, context.DisplayAspectRatio(), 0.01f, 100.0f);

            // Camera matrix
            ViewMatrix = Matrix4x4.CreateLookAt(-forward * distance, Vector3.Zero, up);
        }
    }
}
