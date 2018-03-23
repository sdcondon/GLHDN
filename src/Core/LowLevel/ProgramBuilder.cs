﻿namespace OpenGlHelpers.Core
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

        private List<ShaderType> shaderTypes = new List<ShaderType>();
        private List<string> shaderSources = new List<string>();
        private string[] uniformNames;

        public static ProgramBuilder Colored { get; } =
            new ProgramBuilder()
                .WithStandardShader(ShaderType.VertexShader, $"{CommonShaderResourceNamePrefix}.Colored.Vertex.glsl")
                .WithStandardShader(ShaderType.FragmentShader, $"{CommonShaderResourceNamePrefix}.Colored.Fragment.glsl")
                .WithUniforms("MVP", "V", "M", "LightPosition_worldspace", "LightColor", "LightPower", "AmbientLightColor");

        public static ProgramBuilder Textured { get; } =
            new ProgramBuilder()
                .WithStandardShader(ShaderType.VertexShader, $"{CommonShaderResourceNamePrefix}.Textured.Vertex.glsl")
                .WithStandardShader(ShaderType.FragmentShader, $"{CommonShaderResourceNamePrefix}.Textured.Fragment.glsl")
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

        public ProgramBuilder WithStandardShader(ShaderType shaderType, string resourceName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
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

        public Program Create()
        {
            return new Program(shaderTypes.ToArray(), shaderSources.ToArray(), uniformNames);
        }
    }
}
