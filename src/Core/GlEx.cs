namespace OpenGlHelpers.Core
{
    using OpenGL;
    using System;
    using System.Numerics;

    public static class GlEx
    {
        public static void BufferData(uint buffer, BufferTarget target, BufferUsage usage, Vector3[] data)
        {
            Gl.BindBuffer(target, buffer);
            Gl.BufferData(target, (uint)(sizeof(float) * 3 * data.Length), data, BufferUsage.StaticDraw);
        }

        public static void BufferData(uint buffer, BufferTarget target, BufferUsage usage, Vector2[] data)
        {
            Gl.BindBuffer(target, buffer);
            Gl.BufferData(target, (uint)(sizeof(float) * 2 * data.Length), data, BufferUsage.StaticDraw);
        }

        public static void BufferData(uint buffer, BufferTarget target, BufferUsage usage, uint[] data)
        {
            Gl.BindBuffer(target, buffer);
            Gl.BufferData(target, (uint)(sizeof(uint) * data.Length), data, BufferUsage.StaticDraw);
        }

        public static void BufferData(uint buffer, BufferTarget target, BufferUsage usage, float[] data)
        {
            Gl.BindBuffer(target, buffer);
            Gl.BufferData(target, (uint)(sizeof(float) * data.Length), data, BufferUsage.StaticDraw);
        }

        public static void BufferSubData(uint buffer, uint offset, float data)
        {
            Gl.NamedBufferSubData(buffer, new IntPtr(offset * sizeof(float)), (uint)sizeof(float), data);
        }
    }
}
