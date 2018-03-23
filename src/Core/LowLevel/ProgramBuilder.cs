namespace OpenGlHelpers.Core
{
    using OpenGL;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;

    public sealed class ProgramBuilder
    {
        private const string CommonShaderResourceNamePrefix = "OpenGlHelpers.Core.LowLevel.CommonShaders";

        private List<uint> shaderIds = new List<uint>();
        private string[] uniformNames;

        public static ProgramBuilder Colored { get; } =
            new ProgramBuilder()
                .WithStandardShader(ShaderType.VertexShader, $"{CommonShaderResourceNamePrefix}.Colored.Vertex.glsl")
                .WithStandardShader(ShaderType.FragmentShader, $"{CommonShaderResourceNamePrefix}.Colored.Fragment.glsl")
                .WithUniforms("MVP", "V", "M", "myTextureSampler", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor");

        public static ProgramBuilder Textured { get; } =
            new ProgramBuilder()
                .WithStandardShader(ShaderType.VertexShader, $"{CommonShaderResourceNamePrefix}.Textured.Vertex.glsl")
                .WithStandardShader(ShaderType.FragmentShader, $"{CommonShaderResourceNamePrefix}.Textured.Fragment.glsl")
                .WithUniforms("MVP", "V", "M", "myTextureSampler", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor");

        public ProgramBuilder WithShaderFromFile(ShaderType shaderType, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                AddShader(shaderType, stream);
            }

            return this;
        }
        
        public ProgramBuilder WithShaderFromEmbeddedResource(ShaderType shaderType, string resourceName)
        {
            using (var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName))
            {
                AddShader(shaderType, stream);
            }

            return this;
        }

        public ProgramBuilder WithStandardShader(ShaderType shaderType, string resourceName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                AddShader(shaderType, stream);
            }

            return this;
        }

        public ProgramBuilder WithUniforms(params string[] uniformNames)
        {
            this.uniformNames = uniformNames;
            return this;
        }

        public Program Create()
        {
            Trace.WriteLine("Linking program", "OPENGL");

            var programID = Gl.CreateProgram();

            foreach (var shaderId in shaderIds)
            {
                Gl.AttachShader(programID, shaderId);
            }

            Gl.LinkProgram(programID);

            // Check the program
            Gl.GetProgram(programID, ProgramProperty.InfoLogLength, out var infoLogLength);
            if (infoLogLength > 0)
            {
                var error = new StringBuilder(infoLogLength);
                Gl.GetProgramInfoLog(programID, infoLogLength, out _, error);
                Trace.TraceError(error.ToString());
            }

            foreach (var shaderId in shaderIds)
            {
                Gl.DetachShader(programID, shaderId);
                Gl.DeleteShader(shaderId);
            }

            return new Program(programID, uniformNames);
        }

        private void AddShader(ShaderType shaderType, Stream sourceStream)
        {
            // Create the shader
            var shaderId = Gl.CreateShader(shaderType);

            // Read the shader code from the file
            string shaderSource = null;

            using (var resourceReader = new StreamReader(sourceStream))
            {
                shaderSource = resourceReader.ReadToEnd();
            }

            // Compile shader
            Trace.WriteLine($"Compiling shader", "OPENGL");
            Gl.ShaderSource(shaderId, new[] { shaderSource });
            Gl.CompileShader(shaderId);

            // Check shader
            Gl.GetShader(shaderId, ShaderParameterName.InfoLogLength, out int infoLogLength);
            if (infoLogLength > 0)
            {
                var error = new StringBuilder(infoLogLength);
                Gl.GetShaderInfoLog(shaderId, 100, out int length, error);
                Trace.WriteLine(error);
            }

            shaderIds.Add(shaderId);
        }
    }
}
