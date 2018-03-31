namespace OpenGlHelpers.Core
{
    using OpenGL;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// Renderer class for static geometry.
    /// </summary>
    public class StaticTexuredRenderer : IRenderer
    {
        private uint[] textures;
        private ProgramBuilder programBuilder;
        private Program program;
        private VertexArrayObjectBuilder vertexArrayObjectBuilder;
        private VertexArrayObject vertexArrayObject;

        public StaticTexuredRenderer(
            IList<Vector3> vertexPositions,
            IList<Vector3> vertexNormals,
            IList<Vector2> vertexTextureCoordinates,
            IList<uint> indices)
        {
            this.programBuilder = ProgramBuilder.Textured;
            this.vertexArrayObjectBuilder = new VertexArrayObjectBuilder(PrimitiveType.Triangles)
                .WithBuffer(BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexPositions.ToArray())
                .WithBuffer(BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexTextureCoordinates.ToArray())
                .WithBuffer(BufferTarget.ArrayBuffer, BufferUsage.StaticDraw, vertexNormals.ToArray())
                .WithIndex(indices.ToArray());
        }

        /// <inheritdoc />
        public void ContextCreated(DeviceContext deviceContext)
        {
            this.textures = new uint[1];
            this.textures[0] = TextureLoader.LoadDDS("Assets/uvmap.DDS");

            this.program = this.programBuilder.Build();
            this.vertexArrayObject = this.vertexArrayObjectBuilder.Build();
        }

        /// <inheritdoc />
        public void Render(DeviceContext deviceContext, Matrix4x4 model, Matrix4x4 view, Matrix4x4 projection)
        {
            program.UseWithUniformValues(
                Matrix4x4.Transpose(view * projection),
                Matrix4x4.Transpose(view),
                Matrix4x4.Transpose(model),
                0,
                new Vector3(4, 4, 4),
                new Vector3(1, 1, 1),
                30f,
                new Vector3(0.3f, 0.3f, 0.3f));

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2d, textures[0]);

            this.vertexArrayObject.Draw();
        }

        /// <inheritdoc />
        public void ContextDestroying(DeviceContext deviceContext)
        {
            this.vertexArrayObject.Dispose();
            this.program.Dispose();
            Gl.DeleteTextures(textures);
        }
    }
}
