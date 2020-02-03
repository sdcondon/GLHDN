using OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;

namespace GLHDN.Core
{
    /// <summary>
    /// Represents a compiled OpenGL program.
    /// </summary>
    public sealed class GlProgram : IDisposable
    {
        private readonly uint id;
        private readonly int[] uniformIds;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlProgram"/> class.
        /// </summary>
        /// <param name="shaderSpecs">Specifications for each of the shaders to be included in the program.</param>
        /// <param name="uniforms">The names of the unifoms used by the shaders.</param>
        internal GlProgram(IEnumerable<(ShaderType Type, string Source)> shaderSpecs, string[] uniforms)
        {
            GlExt.ThrowIfNoCurrentContext();

            // Create program
            this.id = Gl.CreateProgram();

            // Compile shaders
            var shaderIds = new List<uint>();
            foreach (var shaderSpec in shaderSpecs)
            {
                // Create shader
                var shaderId = Gl.CreateShader(shaderSpec.Type);

                // Compile shader
                GlExt.DebugWriteLine("Compiling shader");
                Gl.ShaderSource(shaderId, new[] { shaderSpec.Source });
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
            GlExt.DebugWriteLine("Linking program");
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

        /// <summary>
        /// Finalizes an instance of the <see cref="GlProgram"/> class.
        /// </summary>
        ~GlProgram() => Dispose(false);

        /// <summary>
        /// Installs the program as part of the current rendering state and sets the current uniform values (using the default uniform block).
        /// </summary>
        /// <param name="values">The uniform values (in the order in which they were registered).</param>
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
                        var value = new[]
                        {
                            m.M11, m.M12, m.M13, m.M14,
                            m.M21, m.M22, m.M23, m.M24,
                            m.M31, m.M32, m.M33, m.M34,
                            m.M41, m.M42, m.M43, m.M44,
                        };
                        Gl.UniformMatrix4(uniformIds[i], true, value);
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
                        throw new ArgumentException($"Contains value of unsupported type {values[i].GetType()} at index {i}", nameof(values));
                }
            }
        }

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            if (DeviceContext.GetCurrentContext() != IntPtr.Zero)
            {
                Gl.DeleteProgram(this.id);
            }
        }
    }
}
