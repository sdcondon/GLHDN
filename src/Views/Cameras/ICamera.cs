namespace GLHDN.Views
{
    using System;
    using System.Numerics;

    public interface ICamera : IViewProjection
    {
        Vector3 Position { get; }

        void Update(TimeSpan elapsed, View controls);
    }
}
