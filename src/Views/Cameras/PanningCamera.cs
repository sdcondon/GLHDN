namespace GLHDN.Views
{
    using System;
    using System.Numerics;

    public class PanningCamera : ICamera
    {
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
        /// <param name="angle">The angle between the camera's view direction and the Z-axis.</param>
        public PanningCamera(
            float fieldOfViewRadians,
            float nearPlaneDistance,
            float farPlaneDistance,
            Vector3 initialTarget,
            float movementSpeed,
            float angle)
        {
            FieldOfViewRadians = fieldOfViewRadians;
            NearPlaneDistance = nearPlaneDistance;
            FarPlaneDistance = farPlaneDistance;
            target = initialTarget;
            this.movementSpeed = movementSpeed;
            this.Angle = angle;
        }

        public float FieldOfViewRadians { get; set; } // = (float)Math.PI / 4.0f;

        public float NearPlaneDistance { get; set; } // = 0.01f;

        public float FarPlaneDistance { get; set; } // = 100f;

        private float ZoomDefaultDistance { get; set; } = 600f;

        private float ZoomBase { get; set; } = 0.999f;

        public float Distance => (float)(ZoomDefaultDistance * Math.Pow(ZoomBase, zoomLevel));

        public float Angle { get; set; }

        /// <inheritdoc />
        public Vector3 Position => target + Vector3.Transform(Vector3.UnitZ * Distance, Matrix4x4.CreateRotationX(Angle));

        /// <inheritdoc />
        public Matrix4x4 View { get; private set; }

        /// <inheritdoc />
        public Matrix4x4 Projection { get; private set; }

        /// <inheritdoc />
        public void Update(TimeSpan elapsed, View view)
        {
            if (view.KeysDown.Contains('W'))
            {
                target += movementSpeed * Vector3.UnitY;
            }
            if (view.KeysDown.Contains('S'))
            {
                target -= movementSpeed * Vector3.UnitY;
            }
            if (view.KeysDown.Contains('D'))
            {
                target += movementSpeed * Vector3.UnitX;
            }
            if (view.KeysDown.Contains('A'))
            {
                target -= movementSpeed * Vector3.UnitX;
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
