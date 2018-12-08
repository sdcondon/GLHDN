namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.InteropServices;

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
            { typeof(uint), (VertexAttribType.UnsignedInt, 1) }
        };

        public readonly IntPtr offset;
        public readonly int stride;
        public readonly VertexAttribType type;
        public readonly int multiple;

        internal GlVertexAttributeInfo(VertexAttribType type, int multiple, int offset, int stride)
        {
            this.offset = new IntPtr(offset);
            this.stride = stride;
            this.type = type;
            this.multiple = multiple;
        }

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