namespace GLHDN.Views
{
    using System;
    using System.Numerics;

    public struct Ray
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ray"/> class that originates from a given camera position,
        /// and points in the direction as indicated by the current mouse cursor position in a view.
        /// </summary>
        /// <param name="camera">The camera</param>
        /// <param name="view">The view</param>
        public Ray(ICamera camera, View view)
        {
            // http://antongerdelan.net/opengl/raycasting.html
            float x = - (2.0f * view.CursorPosition.X) / view.Width; // TODO: why did i have to negate this?
            float y = (2.0f * view.CursorPosition.Y) / view.Height; // TODO: why did i have to negate this?
            var ray_clip = new Vector3(x, y, -1.0f);

            Matrix4x4.Invert(camera.Projection, out var projInverse);
            var ray_eye = Vector4.Transform(ray_clip, projInverse);
            ray_eye = new Vector4(ray_eye.X, ray_eye.Y, -1.0f, 0.0f);

            Matrix4x4.Invert(camera.View, out var viewInverse);
            var ray_wor = Vector4.Transform(ray_eye, viewInverse);
            Direction = Vector3.Normalize(new Vector3(ray_wor.X, ray_wor.Y, ray_wor.Z));

            Origin = camera.Position; // todo: do we really need a position?
        }

        public Vector3 Origin { get; private set; }

        public Vector3 Direction { get; private set; }

        public static Vector3[] GetIntersections(Ray ray, Vector3 sphereCentre, float sphereRadius)
        {
            var offset = ray.Origin - sphereCentre;
            var b = Vector3.Dot(ray.Direction, offset);
            var c = Vector3.Dot(offset, offset) - sphereRadius * sphereRadius;
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

        public static Vector3? GetIntersection(Ray ray, Plane plane)
        {
            var numerator = Plane.DotNormal(plane, (plane.Normal * plane.D - ray.Origin));
            var denominator = Plane.DotNormal(plane, ray.Direction);

            if (denominator != 0)
            {
                var distanceAlongRay = numerator / denominator;
                return ray.Origin + ray.Direction * distanceAlongRay;
            }

            return null;
        }
    }
}
