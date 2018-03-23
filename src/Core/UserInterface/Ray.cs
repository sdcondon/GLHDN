namespace OpenGlHelpers.Core
{
    using System;
    using System.Numerics;

    public struct Ray
    {
        public Ray(ICamera camera, IUiContext uiContext)
        {
            // http://antongerdelan.net/opengl/raycasting.html
            float x = - (2.0f * uiContext.CursorMovementX) / uiContext.DisplayWidth;
            float y = (2.0f * uiContext.CursorMovementY) / uiContext.DisplayHeight;
            var ray_clip = new Vector4(x, y, -1.0f, 1.0f);

            Matrix4x4.Invert(camera.ProjectionMatrix, out var projInverse);
            var ray_eye = Vector4.Transform(ray_clip, projInverse);
            ray_eye = new Vector4(ray_eye.X, ray_eye.Y, -1.0f, 0.0f);

            Matrix4x4.Invert(camera.ViewMatrix, out var viewInverse);
            var ray_wor = Vector4.Transform(ray_eye, viewInverse);
            Direction = Vector3.Normalize(new Vector3(ray_wor.X, ray_wor.Y, ray_wor.Z));

            Origin = camera.Position; // todo: do we need a position?
        }

        public Vector3 Origin { get; private set; }

        public Vector3 Direction { get; private set; }

        public static Vector3[] Intersection(Ray ray, Vector3 centre, float radius)
        {
            var offset = ray.Origin - centre;
            var b = Vector3.Dot(ray.Direction, offset);
            var c = Vector3.Dot(offset, offset) - radius * radius;
            var discriminant = b * b - c;

            if (discriminant < 0)
            {
                return new Vector3[0];
            }
            else if (discriminant > 0)
            {
                var t = new[] { -b + (float)Math.Sqrt(discriminant), -b - (float)Math.Sqrt(discriminant) };
                Array.Sort(t);
                return new[] { ray.Origin + t[0] * ray.Direction, ray.Origin + t[1] * ray.Direction };
            }
            else
            {
                return new[] { ray.Origin + -b * ray.Direction };
            }
        }
    }
}
