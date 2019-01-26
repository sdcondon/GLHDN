namespace GLHDN.Views
{
    using System;
    using System.Numerics;

    public class FirstPersonCamera : ICamera
    {
        private float horizontalAngle;
        private float verticalAngle;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirstPersonCamera"/> class.
        /// </summary>
        public FirstPersonCamera(
            float movementSpeed,
            float rotationSpeed,
            float fieldOfViewRadians,
            float nearPlaneDistance,
            float farPlaneDistance,
            Vector3 initialPosition,
            float initialHorizontalAngleRadians,
            float initialVerticalAngleRadians)
        {
            this.MovementSpeed = movementSpeed;
            this.RotationSpeed = rotationSpeed;
            this.FieldOfViewRadians = fieldOfViewRadians;
            this.NearPlaneDistance = nearPlaneDistance;
            this.FarPlaneDistance = farPlaneDistance;
            this.Position = initialPosition;
            this.horizontalAngle = initialHorizontalAngleRadians;
            this.verticalAngle = initialVerticalAngleRadians;
        }

        /// <summary>
        /// Gets or sets the movement speed of the camera in units per second.
        /// </summary>
        public float MovementSpeed { get; set; } // = 3.0f;

        /// <summary>
        /// Gets or sets the "rotation speed" of the camera - the multiplicand of the mouse cursor offset to radians.
        /// </summary>
        public float RotationSpeed { get; set; } // = 0.005f;

        /// <summary>
        /// Gets or sets the field of view of the camera, in radians.
        /// </summary>
        public float FieldOfViewRadians { get; set; } // = (float)Math.PI / 4.0f;

        /// <summary>
        /// Gets or sets the distance of the near plane from the camera.
        /// </summary>
        private float NearPlaneDistance { get; set; } // = 0.1f;

        /// <summary>
        /// Gets or sets the distance of the far plane from the camera.
        /// </summary>
        private float FarPlaneDistance { get; set; } // = 100f;

        /// <inheritdoc />
        /// <remarks>Should be private, but used by Ray..</remarks>
        public Vector3 Position { get; private set; } // = new Vector3(0, 0, -300);

        /// <inheritdoc />
        public Matrix4x4 View { get; private set; }

        /// <inheritdoc />
        public Matrix4x4 Projection { get; private set; }

        /// <inheritdoc />
        public void Update(TimeSpan elapsed, View view)
        {
            // Compute new orientation
            var xDiff = view.CursorMovement.X;
            if (Math.Abs(xDiff) < 2) xDiff = 0;
            horizontalAngle += RotationSpeed * xDiff;

            var yDiff = view.CursorMovement.Y;
            if (Math.Abs(yDiff) < 2) yDiff = 0;
            verticalAngle += RotationSpeed * yDiff;
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
            if (view.PressedKeys.Contains('W'))
            {
                Position += direction * (float)elapsed.TotalSeconds * MovementSpeed;
            }
            // Move backward
            if (view.PressedKeys.Contains('S'))
            {
                Position -= direction * (float)elapsed.TotalSeconds * MovementSpeed;
            }
            // Strafe right
            if (view.PressedKeys.Contains('D'))
            {
                Position += right * (float)elapsed.TotalSeconds * MovementSpeed;
            }
            // Strafe left
            if (view.PressedKeys.Contains('A'))
            {
                Position -= right * (float)elapsed.TotalSeconds * MovementSpeed;
            }

            Projection = Matrix4x4.CreatePerspectiveFieldOfView(
                FieldOfViewRadians,
                view.AspectRatio,
                0.01f,
                1000.0f);

            // Camera matrix
            View = Matrix4x4.CreateLookAt(
                Position,             // Camera is here
                Position + direction, // and looks here : at the same position, plus "direction"
                up);                  // Head is up (set to 0,-1,0 to look upside-down)
        }
    }
}
