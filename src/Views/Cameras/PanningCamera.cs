namespace GLHDN.Views
{
    using System;
    using System.Numerics;

    public class PanningCamera : ICamera
    {
        private Vector3 target;
        private int zoomLevel = 0;
        private float movementSpeed;

        public PanningCamera(
            float fieldOfViewRadians,
            float nearPlaneDistance,
            float farPlaneDistance,
            Vector3 initialTarget,
            float movementSpeed)
        {
            FieldOfViewRadians = fieldOfViewRadians;
            NearPlaneDistance = nearPlaneDistance;
            FarPlaneDistance = farPlaneDistance;
            target = initialTarget;
            this.movementSpeed = movementSpeed;
        }

        public float FieldOfViewRadians { get; set; } // = (float)Math.PI / 4.0f;

        public float NearPlaneDistance { get; set; } // = 0.01f;

        public float FarPlaneDistance { get; set; } // = 100f;

        private float ZoomDefaultDistance { get; set; } = 600f;

        private float ZoomBase { get; set; } = 0.999f;

        public float Distance => (float)(ZoomDefaultDistance * Math.Pow(ZoomBase, zoomLevel));

        /// <inheritdoc />
        public Vector3 Position => target + Vector3.UnitZ * Distance;

        /// <inheritdoc />
        public Matrix4x4 View { get; private set; }

        /// <inheritdoc />
        public Matrix4x4 Projection { get; private set; }

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
