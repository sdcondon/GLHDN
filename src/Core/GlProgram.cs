namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
    using System.Text;

    /// <summary>
    /// Represents a compiled OpenGL program.
    /// </summary>
    public sealed class GlProgram : IDisposable
    {
        private readonly uint id;
        private readonly int[] uniformIds;

        internal GlProgram(IList<ShaderType> shaderTypes, IList<string> shaderSources, string[] uniforms)
        {
            // Create program
            this.id = Gl.CreateProgram();

            // Compile shaders
            var shaderIds = new List<uint>();
            for (int i = 0; i < shaderTypes.Count; i++)
            {
                // Create shader
                var shaderId = Gl.CreateShader(shaderTypes[i]);

                // Compile shader
                Trace.WriteLine($"Compiling shader", "OPENGL");
                Gl.ShaderSource(shaderId, new[] { shaderSources[i] });
                Gl.CompileShader(shaderId);

                // Check shader
                Gl.GetShader(shaderId, ShaderParameterName.InfoLogLength, out var shaderInfoLogLength);
                if (shaderInfoLogLength > 0)
                {
                    var error = new StringBuilder(shaderInfoLogLength);
                    Gl.GetShaderInfoLog(shaderId, shaderInfoLogLength, out _, error);
                    Trace.WriteLine(error);
                }

                Gl.AttachShader(this.id, shaderId);
                shaderIds.Add(shaderId);
            }

            // Link & check program
            Trace.WriteLine("Linking program", "OPENGL");
            Gl.LinkProgram(this.id);
            Gl.GetProgram(this.id, ProgramProperty.InfoLogLength, out var programInfoLogLength);
            if (programInfoLogLength > 0)
            {
                var error = new StringBuilder(programInfoLogLength);
                Gl.GetProgramInfoLog(this.id, programInfoLogLength, out _, error);
                Trace.TraceError(error.ToString());
            }

            // Detach and delete shaders
            foreach (var shaderId in shaderIds)
            {
                Gl.DetachShader(this.id, shaderId); // Line not in superbible?
                Gl.DeleteShader(shaderId);
            }

            // Get uniform IDs
            this.uniformIds = uniforms.Select(x => Gl.GetUniformLocation(this.id, x)).ToArray();
        }

        ~GlProgram()
        {
            Dispose(false);
        }

        /// <summary>
        /// Installs the program as part of the current rendering state and sets the current uniform values (using the default uniform block).
        /// </summary>
        /// <param name="values">The uniform values (in the order in which they were created).</param>
        public void UseWithUniformValues(params object[] values)
        {
            Gl.UseProgram(this.id);
            for (int i = 0; i < uniformIds.Length; i++)
            {
                switch (values[i])
                {
                    case Matrix4x4 m:
                        // NB: If transpose argument is false, OpenGL expects arrays in column major order.
                        // We set transpose to true for readability (and thus maintainability) - so that
                        // our little matrix array below looks right.
                        Gl.UniformMatrix4(
                            uniformIds[i],
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
                        Gl.Uniform3(uniformIds[i], v.X, v.Y, v.Z);
                        break;
                    case float f:
                        Gl.Uniform1(uniformIds[i], f);
                        break;
                    case int iv:
                        Gl.Uniform1(uniformIds[i], iv);
                        break;
                    case uint u:
                        Gl.Uniform1(uniformIds[i], u);
                        break;
                    case long l:
                        Gl.Uniform1(uniformIds[i], l);
                        break;
                    default:
                        throw new ArgumentException("Contains value of unsupported type", nameof(values));
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            Gl.DeleteProgram(this.id);
        }
    }
}
