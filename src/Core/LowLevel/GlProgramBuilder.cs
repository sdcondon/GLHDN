namespace OpenGlHelpers.Core.LowLevel
{
    using OpenGL;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Builder class for <see cref="GlProgram"/> objects that presents a fluent-ish interface.
    /// </summary>
    public sealed class GlProgramBuilder
    {
        private List<ShaderType> shaderTypes = new List<ShaderType>();
        private List<string> shaderSources = new List<string>();
        private string[] uniformNames;

        public GlProgramBuilder WithShaderFromFile(ShaderType shaderType, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                shaderSources.Add(reader.ReadToEnd());
            }

            shaderTypes.Add(shaderType);
            return this;
        }
        
        public GlProgramBuilder WithShaderFromEmbeddedResource(ShaderType shaderType, string resourceName)
        {
            using (var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                shaderSources.Add(reader.ReadToEnd());
            }

            shaderTypes.Add(shaderType);
            return this;
        }

        public GlProgramBuilder WithUniforms(params string[] uniformNames)
        {
            this.uniformNames = uniformNames;
            return this;
        }

        public GlProgram Build()
        {
            return new GlProgram(shaderTypes, shaderSources, uniformNames);
        }
    }
}
