namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Container for information about an OpenGL vertex attribute.
    /// </summary>
    public struct GlVertexAttributeInfo
    {
        /// <summary>
        /// A mapping of .NET types to equivalent primitive OpenGL attribute info for them.
        /// </summary>
        private static readonly Dictionary<Type, (VertexAttribType type, int count)> KnownTypes = new Dictionary<Type, (VertexAttribType, int)>()
        {
            { typeof(Vector4), (VertexAttribType.Float, 4) },
            { typeof(Vector3), (VertexAttribType.Float, 3) },
            { typeof(Vector2), (VertexAttribType.Float, 2) },
            { typeof(float), (VertexAttribType.Float, 1) },
            { typeof(uint), (VertexAttribType.UnsignedInt, 1) },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="GlVertexAttributeInfo"/> struct.
        /// </summary>
        /// <param name="type">The OpenGL type for the vertex attribute.</param>
        /// <param name="multiple">The multiple (of the OpenGL type) for the attribute.</param>
        /// <param name="offset">The offset from the start of the buffer to the attribute for the first vertex in the buffer.</param>
        /// <param name="stride">The offset from the attribute for one vertex in the buffer to the next.</param>
        internal GlVertexAttributeInfo(VertexAttribType type, int multiple, int offset, int stride)
        {
            this.Type = type;
            this.Multiple = multiple;
            this.Offset = new IntPtr(offset);
            this.Stride = stride;
        }

        /// <summary>
        /// Gets the OpenGL type for the vertex attribute.
        /// </summary>
        public VertexAttribType Type { get; }

        /// <summary>
        /// Gets the multiple (of the OpenGL type) for the attribute.
        /// </summary>
        public int Multiple { get; }

        /// <summary>
        /// Gets the offset from the start of the buffer to the attribute for the first vertex in the buffer.
        /// </summary>
        public IntPtr Offset { get; }

        /// <summary>
        /// Gets the offset from the attribute for one vertex in the buffer to the next.
        /// </summary>
        public int Stride { get; }

        /// <summary>
        /// Returns attribute info for a given (blittable) type.
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns>An array of attribute info.</returns>
        internal static GlVertexAttributeInfo[] ForType(Type t)
        {
            var attributes = new List<GlVertexAttributeInfo>();
            ForType(t, attributes, 0, Marshal.SizeOf(t));
            return attributes.ToArray();
        }

        private static void ForType(Type t, List<GlVertexAttributeInfo> attributes, int offset, int stride)
        {
            if (KnownTypes.TryGetValue(t, out var glInfo))
            {
                attributes.Add(new GlVertexAttributeInfo(glInfo.type, glInfo.count, offset, stride));
            }
            else if (!t.IsValueType || t.IsAutoLayout)
            {
                throw new ArgumentException("Unsupported type - passed type must be blittable");
            }
            else
            {
                foreach (var field in t.GetFields())
                {
                    ForType(field.FieldType, attributes, offset + (int)Marshal.OffsetOf(t, field.Name), stride);
                }
            }
        }
    }
}