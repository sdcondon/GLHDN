namespace GLHDN.Views.Renderables.ReactivePrimitives
{
    using System.Numerics;

    /// <summary>
    /// Container struct for the attributes of a vertex of a primitive.
    /// </summary>
    public struct PrimitiveVertex
    {
        /// <summary>
        /// Gets the position of the vertex.
        /// </summary>
        public readonly Vector3 Position;

        /// <summary>
        /// Gets the color of the vertex.
        /// </summary>
        public readonly Vector4 Color;

        /// <summary>
        /// Gets the normal vector of the vertex.
        /// </summary>
        public readonly Vector3 Normal;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveVertex"/> struct.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="color">The color of the vertex.</param>
        /// <param name="normal">The normal vector of the vertex.</param>
        public PrimitiveVertex(Vector3 position, Vector4 color, Vector3 normal)
        {
            Position = position;
            Color = color;
            Normal = normal;
        }
    }
}
