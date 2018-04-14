namespace GLHDN.Views
{
    using GLHDN.Core;
    using System;
    using System.Numerics;

    public class FirstPersonCamera : ICamera
    {
        private const float speed = 3.0f; // 3 units / second
        private const float mouseSpeed = 0.005f;

        // Initial position : on -Z
        private Vector3 position = new Vector3(0, 0, -3);

        // Initial horizontal angle : toward +Z
        private float horizontalAngle = 0.0f;

        // Initial vertical angle : none
        private float verticalAngle = 0.0f;

        // Initial Field of View in radians
        private float initialFoV = (float)Math.PI / 4.0f;

        /// <inheritdoc />
        public Vector3 Position => position;

        /// <inheritdoc />
        public Matrix4x4 View
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public Matrix4x4 Projection
        {
            get;
            private set;
        }

        public void Update(TimeSpan elapsed, IUiContext context)
        {
            // Compute new orientation
            var xDiff = context.CursorMovementX;
            if (Math.Abs(xDiff) < 2) xDiff = 0;
            horizontalAngle += mouseSpeed * xDiff;

            var yDiff = context.CursorMovementY;
            if (Math.Abs(yDiff) < 2) yDiff = 0;
            verticalAngle += mouseSpeed * yDiff;
            verticalAngle = Math.Max(-(float)Math.PI / 2, Math.Min(verticalAngle, (float)Math.PI / 2));

            // Direction : Spherical coordinates to Cartesian coordinates conversion
            var direction = new Vector3(
                (float)(Math.Cos(verticalAngle) * Math.Sin(horizontalAngle)),
                (float)Math.Sin(verticalAngle),
                (float)(Math.Cos(verticalAngle) * Math.Cos(horizontalAngle)));

            // Right vector
            var right = new Vector3(
                (float)Math.Sin(horizontalAngle - 3.14f / 2.0f),
                0,
                (float)Math.Cos(horizontalAngle - 3.14f / 2.0f));

            // Up vector
            var up = Vector3.Cross(right, direction);

            // Move forward
            if (context.PressedKeys.Contains('W'))
            {
                position += direction * (float)elapsed.TotalSeconds * speed;
            }
            // Move backward
            if (context.PressedKeys.Contains('S'))
            {
                position -= direction * (float)elapsed.TotalSeconds * speed;
            }
            // Strafe right
            if (context.PressedKeys.Contains('D'))
            {
                position += right * (float)elapsed.TotalSeconds * speed;
            }
            // Strafe left
            if (context.PressedKeys.Contains('A'))
            {
                position -= right * (float)elapsed.TotalSeconds * speed;
            }

            float FoV = initialFoV;

            // Projection matrix : 45° Field of View, 4:3 ratio, display range : 0.01 unit <-> 100 units
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(
                FoV,
                context.DisplayAspectRatio(),
                0.01f,
                100.0f);

            // Camera matrix
            View = Matrix4x4.CreateLookAt(
                position,             // Camera is here
                position + direction, // and looks here : at the same position, plus "direction"
                up);                  // Head is up (set to 0,-1,0 to look upside-down)
        }
    }
}
