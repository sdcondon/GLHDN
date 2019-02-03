namespace GLHDN.Views
{
    using System;
    using System.Numerics;

    public class OrbitCamera : ICamera
    {
        private Vector3 forward = new Vector3(0f, 0f, 1f);
        private Vector3 up = new Vector3(0f, 1f, 0f);
        private int zoomLevel = 0;

        public OrbitCamera(
            float rotationSpeedBase,
            float rollSpeed,
            float fieldOfViewRadians,
            float nearPlaneDistance,
            float farPlaneDistance)
        {
            RotationSpeedBase = rotationSpeedBase;
            RollSpeed = rollSpeed;
            FieldOfViewRadians = fieldOfViewRadians;
            NearPlaneDistance = nearPlaneDistance;
            FarPlaneDistance = farPlaneDistance;
        }

        public float RotationSpeedBase { get; set; } // = 0.01f;

        public float RotationSpeed => RotationSpeedBase * (Distance - ZoomMinDistance) / ZoomDefaultDistance;

        public float RollSpeed { get; set; } // = 0.01f;

        public float FieldOfViewRadians { get; set; } // = (float)Math.PI / 4.0f;

        public float NearPlaneDistance { get; set; } // = 0.01f;

        public float FarPlaneDistance { get; set; } // = 100f;

        private float ZoomDefaultDistance { get; set; } = 1.5f;

        private float ZoomBase { get; set; } = 0.999f;

        private float ZoomMinDistance => 1f + NearPlaneDistance;

        public float Distance => (float)(ZoomMinDistance + ZoomDefaultDistance * Math.Pow(ZoomBase, zoomLevel));

        /// <inheritdoc />
        public Vector3 Position => -forward * Distance;

        /// <inheritdoc />
        public Matrix4x4 View { get; private set; }

        /// <inheritdoc />
        public Matrix4x4 Projection { get; private set; }

        public void Update(TimeSpan elapsed, View view)
        {
            // Pan up - rotate forward and up around their cross product
            if (view.KeysDown.Contains('W'))
            {
                var t = Matrix4x4.CreateFromAxisAngle(Vector3.Cross(forward, up), -RotationSpeed);
                forward = Vector3.Transform(forward, t);
                up = Vector3.Transform(up, t);
            }
            // Pan down - rotate forward and up around their cross product
            if (view.KeysDown.Contains('S'))
            {
                var t = Matrix4x4.CreateFromAxisAngle(Vector3.Cross(forward, up), RotationSpeed);
                forward = Vector3.Normalize(Vector3.Transform(forward, t));
                up = Vector3.Normalize(Vector3.Transform(up, t));
            }
            // Pan right - rotate forward around up
            if (view.KeysDown.Contains('D'))
            {
                forward = Vector3.Normalize(Vector3.Transform(forward, Matrix4x4.CreateFromAxisAngle(up, RotationSpeed)));
            }
            // Pan left - rotate forward around up
            if (view.KeysDown.Contains('A'))
            {
                forward = Vector3.Normalize(Vector3.Transform(forward, Matrix4x4.CreateFromAxisAngle(up, -RotationSpeed)));
            }
            // Roll right - rotate up around forward
            if (view.KeysDown.Contains('Q'))
            {
                up = Vector3.Normalize(Vector3.Transform(up, Matrix4x4.CreateFromAxisAngle(forward, -RollSpeed)));
            }
            // Roll left - rotate up around forward
            if (view.KeysDown.Contains('E'))
            {
                up = Vector3.Normalize(Vector3.Transform(up, Matrix4x4.CreateFromAxisAngle(forward, RollSpeed)));
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
            View = Matrix4x4.CreateLookAt(Position, Vector3.Zero, up);
        }
    }
}
