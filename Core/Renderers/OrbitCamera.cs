namespace OpenGlHelpers.WinForms
{
    using OpenGlHelpers.Core;
    using System;
    using System.Numerics;

    public class OrbitCamera : ICamera
    {
        private const float speed = 3.0f; // 3 units / second
        private const float mouseSpeed = 0.005f;

        private float longitude = 0.0f;
        private float latitude = 0.0f;

        // Initial horizontal angle : toward +Z
        private float horizontalAngle = 0.0f;

        // Initial vertical angle : none
        private float verticalAngle = 0.0f;

        // Field of View, in radians
        public float FieldOfView { get; set; } = (float)Math.PI / 4.0f;

        public Matrix4x4 ViewMatrix
        {
            get;
            private set;
        }

        public Matrix4x4 ProjectionMatrix
        {
            get;
            private set;
        }

        public void Update(TimeSpan elapsed, IContext context)
        {
            // Up vector
            var up = Vector3.UnitZ; //Vector3.Cross(right, direction);

            // Move forward
            if (context.PressedKeys.Contains('w'))
            {
                latitude += (float)elapsed.TotalSeconds * speed;
                if (latitude > Math.PI / 2) latitude = (float)Math.PI / 2;
            }
            // Move backward
            if (context.PressedKeys.Contains('s'))
            {
                latitude -= (float)elapsed.TotalSeconds * speed;
                if (latitude < -Math.PI / 2) latitude = -(float)Math.PI / 2;
            }
            // Strafe right
            if (context.PressedKeys.Contains('d'))
            {
                longitude += (float)elapsed.TotalSeconds * speed;
            }
            // Strafe left
            if (context.PressedKeys.Contains('a'))
            {
                longitude -= (float)elapsed.TotalSeconds * speed;
            }

            var position = new Vector3(
                (float)(Math.Cos(latitude) * Math.Sin(longitude)),
                (float)Math.Sin(latitude),
                (float)(Math.Cos(latitude) * Math.Cos(longitude))) * 5;

            // Projection matrix : 45° Field of View, 4:3 ratio, display range : 0.1 unit <-> 100 units
            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, context.DisplayAspectRatio, 0.01f, 100.0f);

            // Camera matrix
            ViewMatrix = Matrix4x4.CreateLookAt(
                position,     // Camera is here
                Vector3.Zero, //position + direction, // and looks here : at the same position, plus "direction"
                up);          // Head is up (set to 0,-1,0 to look upside-down)
        }
    }
}
