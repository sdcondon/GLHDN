﻿namespace OpenGlHelpers.Core
{
    using System;
    using System.Numerics;

    public interface ICamera
    {
        Vector3 Position { get; }

        Matrix4x4 ViewMatrix { get; }

        Matrix4x4 ProjectionMatrix { get; }

        void Update(TimeSpan elapsed, IUiContext controls);
    }
}
