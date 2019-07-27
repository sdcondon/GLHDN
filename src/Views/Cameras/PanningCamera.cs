namespace GLHDN.Views
{
    using System;
    using System.Numerics;

    public class PanningCamera : ICamera
    {
        private readonly View view;
        private readonly float movementSpeed;

        private Vector3 target;
        private int zoomLevel = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="PanningCamera"/> class.
        /// </summary>
        /// <param name="fieldOfViewRadians">The camera's field of view, in radians.</param>
        /// <param name="nearPlaneDistance">The distance of the near plane from the camera.</param>
        /// <param name="farPlaneDistance">The ditance of the far plane from the camera.</param>
        /// <param name="initialTarget">The initial position at which the camera should point.</param>
        /// <param name="movementSpeed">The movement speed of the camera, in units per update.</param>
        /// <param name="verticalAngle">The angle between the camera's view direction and the Z-axis.</param>
        public PanningCamera(
            View view,
            float fieldOfViewRadians, // = (float)Math.PI / 4.0f;
            float nearPlaneDistance, // = 0.01f;
            float farPlaneDistance, // = 100f;
            Vector3 initialTarget,
            float movementSpeed,
            float verticalAngle)
        {
            this.view = view;
            FieldOfViewRadians = fieldOfViewRadians;
            NearPlaneDistance = nearPlaneDistance;
            FarPlaneDistance = farPlaneDistance;
            target = initialTarget;
            this.movementSpeed = movementSpeed / Distance;
            this.VerticalAngle = verticalAngle;
        }

        public float FieldOfViewRadians { get; set; }

        public float NearPlaneDistance { get; set; }

        public float FarPlaneDistance { get; set; }

        private float ZoomDefaultDistance { get; set; } = 600f;

        private float ZoomBase { get; set; } = 0.999f;

        public float VerticalAngle { get; set; }

        public float VerticalAngleMax { get; set; } = 0.49f * (float)Math.PI;

        public float VerticalRotationSpeed { get; set; } = 1.0f;

        public float HorizontalAngle { get; set; }

        public float HorizontalRotationSpeed { get; set; } = 1.0f;

        public float Distance => (float)(ZoomDefaultDistance * Math.Pow(ZoomBase, zoomLevel));

        /// <inheritdoc />
        public Vector3 Position => target + Vector3.Transform(Vector3.UnitZ * Distance, Matrix4x4.CreateRotationZ(HorizontalAngle) * Matrix4x4.CreateRotationX(VerticalAngle));

        /// <inheritdoc />
        public Matrix4x4 View { get; private set; }

        /// <inheritdoc />
        public Matrix4x4 Projection { get; private set; }

        /// <inheritdoc />
        public void Update(TimeSpan elapsed)
        {
            if (view.KeysDown.Contains('W'))
            {
                target += movementSpeed * (float)elapsed.TotalSeconds * Distance * Vector3.UnitY;
            }
            if (view.KeysDown.Contains('S'))
            {
                target -= movementSpeed * (float)elapsed.TotalSeconds * Distance * Vector3.UnitY;
            }
            if (view.KeysDown.Contains('D'))
            {
                target += movementSpeed * (float)elapsed.TotalSeconds * Distance * Vector3.UnitX;
            }
            if (view.KeysDown.Contains('A'))
            {
                target -= movementSpeed * (float)elapsed.TotalSeconds * Distance * Vector3.UnitX;
            }

            if (view.KeysDown.Contains('R'))
            {
                VerticalAngle -= VerticalRotationSpeed * (float)elapsed.TotalSeconds;
                VerticalAngle = Math.Max(VerticalAngle, 0);
            }
            if (view.KeysDown.Contains('F'))
            {
                VerticalAngle += VerticalRotationSpeed * (float)elapsed.TotalSeconds;
                VerticalAngle = Math.Min(VerticalAngle, VerticalAngleMax);
            }

            if (view.KeysDown.Contains('Q'))
            {
                HorizontalAngle -= HorizontalRotationSpeed * (float)elapsed.TotalSeconds;
                HorizontalAngle = Math.Max(HorizontalAngle, 0);
            }
            if (view.KeysDown.Contains('E'))
            {
                HorizontalAngle += HorizontalRotationSpeed * (float)elapsed.TotalSeconds;
                HorizontalAngle = Math.Min(HorizontalAngle, (float)Math.PI * 2);
            }

            // Zoom
            zoomLevel += view.MouseWheelDelta;

            // Projection matrix
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(
                FieldOfViewRadians,
                view.AspectRatio,
                NearPlaneDistance,
                FarPlaneDistance);

            // Camera matrix
            View = Matrix4x4.CreateLookAt(Position, target, Vector3.UnitY);
        }
    }
}
