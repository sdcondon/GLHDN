namespace OpenGlHelpers.Core
{
    using OpenGL;
    using System;
    using System.Linq;
    using System.Numerics;

    public class Program
    {
        int[] uniformIds;

        public Program(uint id, params string[] uniforms)
        {
            Id = id;
            uniformIds = uniforms.Select(x => Gl.GetUniformLocation(id, x)).ToArray();
        }

        public uint Id { get; private set; }

        public void SetUniformValues(params object[] values)
        {
            for (int i = 0; i < uniformIds.Length; i++)
            {
                SetUniformValue(uniformIds[i], values[i]);
            }
        }

        private void SetUniformValue(int id, object value)
        {
            switch (value)
            {
                case Matrix4x4 m:
                    // NB: If transpose argument is false, OpenGL expects arrays in column major order.
                    // We set transpose to true for readability (and thus maintainability) - so that
                    // our little matrix array below looks right.
                    Gl.UniformMatrix4(
                        id,
                        true,
                        new[]
                        {
                                m.M11, m.M12, m.M13, m.M14,
                                m.M21, m.M22, m.M23, m.M24,
                                m.M31, m.M32, m.M33, m.M34,
                                m.M41, m.M42, m.M43, m.M44
                        });
                    break;
                case Vector3 v:
                    Gl.Uniform3(id, v.X, v.Y, v.Z);
                    break;
                case float f:
                    Gl.Uniform1(id, f);
                    break;
                case int i:
                    Gl.Uniform1(id, i);
                    break;
                default:
                    throw new ArgumentException("Unsupported type", nameof(value));
            }
        }
    }
}
