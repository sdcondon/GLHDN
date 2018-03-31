namespace OpenGlHelpers.Core
{
    using OpenGL;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Builder class for <see cref="Program"/> objects that presents a fluent-ish interface.
    /// </summary>
    public sealed class ProgramBuilder
    {
        private const string StandardShaderResourceNamePrefix = "OpenGlHelpers.Core.LowLevel.StandardShaders";

        private List<ShaderType> shaderTypes = new List<ShaderType>();
        private List<string> shaderSources = new List<string>();
        private string[] uniformNames;

        public static ProgramBuilder Colored { get; } =
            new ProgramBuilder()
                .WithStandardShader(ShaderType.VertexShader, "Colored.Vertex.glsl")
                .WithStandardShader(ShaderType.FragmentShader, "Colored.Fragment.glsl")
                .WithUniforms("MVP", "V", "M", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor");

        public static ProgramBuilder Textured { get; } =
            new ProgramBuilder()
                .WithStandardShader(ShaderType.VertexShader, "Textured.Vertex.glsl")
                .WithStandardShader(ShaderType.FragmentShader, "Textured.Fragment.glsl")
                .WithUniforms("MVP", "V", "M", "myTextureSampler", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor");

        public ProgramBuilder WithShaderFromFile(ShaderType shaderType, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                shaderSources.Add(reader.ReadToEnd());
            }

            shaderTypes.Add(shaderType);
            return this;
        }
        
        public ProgramBuilder WithShaderFromEmbeddedResource(ShaderType shaderType, string resourceName)
        {
            using (var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                shaderSources.Add(reader.ReadToEnd());
            }

            shaderTypes.Add(shaderType);
            return this;
        }

        internal ProgramBuilder WithStandardShader(ShaderType shaderType, string resourceName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{StandardShaderResourceNamePrefix}.{resourceName}"))
            using (var reader = new StreamReader(stream))
            {
                shaderSources.Add(reader.ReadToEnd());
            }

            shaderTypes.Add(shaderType);
            return this;
        }

        public ProgramBuilder WithUniforms(params string[] uniformNames)
        {
            this.uniformNames = uniformNames;
            return this;
        }

        public Program Build()
        {
            return new Program(shaderTypes.ToArray(), shaderSources.ToArray(), uniformNames);
        }
    }
}
