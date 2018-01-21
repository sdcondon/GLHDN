namespace OpenGlHelpers.Core
{
    using OpenGL;
    using System;
    using System.Numerics;

    public class Uniform
    {
        public Uniform(uint programId, string name)
        {
            this.Id = Gl.GetUniformLocation(programId, name);
            this.Name = name;
        }

        public int Id
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public object Value
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                switch (value)
                {
                    case Matrix4x4 m:
                        // NB: If transpose argument is false, OpenGL expects arrays in column major order.
                        // We set transpose to true for readability (and thus maintainability) - so that
                        // our little matrix array below looks right.
                        Gl.UniformMatrix4(
                            this.Id,
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
                        Gl.Uniform3(this.Id, v.X, v.Y, v.Z);
                        break;
                    case float f:
                        Gl.Uniform1(this.Id, f);
                        break;
                    case int i:
                        Gl.Uniform1(this.Id, i);
                        break;
                    default:
                        throw new ArgumentException("Unsupported type", nameof(value));
                }
            }
        }
    }
}
