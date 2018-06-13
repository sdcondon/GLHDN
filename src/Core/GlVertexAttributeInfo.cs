namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public struct VertexAttribInfo
    {
        private static readonly Dictionary<Type, Func<int, int, VertexAttribInfo>> KnownTypes = new Dictionary<Type, Func<int, int, VertexAttribInfo>>()
        {
            { typeof(Vector4), (offset, stride) => new VertexAttribInfo(VertexAttribType.Float, 4, offset, stride) },
            { typeof(Vector3), (offset, stride) => new VertexAttribInfo(VertexAttribType.Float, 3, offset, stride) },
            { typeof(Vector2), (offset, stride) => new VertexAttribInfo(VertexAttribType.Float, 2, offset, stride) },
            { typeof(float), (offset, stride) => new VertexAttribInfo(VertexAttribType.Float, 1, offset, stride) },
            { typeof(uint), (offset, stride) => new VertexAttribInfo(VertexAttribType.UnsignedInt, 1, offset, stride) }
        };

        public readonly IntPtr offset;
        public readonly int stride;
        public readonly VertexAttribType type;
        public readonly int multiple;

        private VertexAttribInfo(VertexAttribType type, int multiple, int offset, int stride)
        {
            this.offset = new IntPtr(offset);
            this.stride = stride;
            this.type = type;
            this.multiple = multiple;
        }

        internal static VertexAttribInfo[] ForType(Type t)
        {
            var attributes = new List<VertexAttribInfo>();
            ForType(t, attributes, 0, Marshal.SizeOf(t));
            return attributes.ToArray();
        }

        private static void ForType(Type t, List<VertexAttribInfo> attributes, int offset, int stride)
        {
            if (KnownTypes.ContainsKey(t))
            {
                attributes.Add(KnownTypes[t](offset, stride));
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