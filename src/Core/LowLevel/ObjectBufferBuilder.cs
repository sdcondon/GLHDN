namespace OpenGlHelpers.Core.LowLevel
{
    using OpenGL;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Builder class for <see cref="ObjectBuffer{T}"/> objects that presents a fluent-ish interface.
    /// </summary>
    public sealed class ObjectBufferBuilder<T>
    {
        private readonly PrimitiveType primitiveType;
        private readonly int verticesPerObject;
        private readonly int objectCapacity;

        private readonly List<Type> attributeTypes = new List<Type>();
        private readonly List<Func<T, int, object>> attributeGetters = new List<Func<T, int, object>>();

        public ObjectBufferBuilder(PrimitiveType primitiveType, int verticesPerObject, int objectCapacity)
        {
            this.primitiveType = primitiveType;
            this.verticesPerObject = verticesPerObject;
            this.objectCapacity = objectCapacity;
        }

        public ObjectBufferBuilder<T> WithAttribute<TAttribute>(Func<T, TAttribute[]> attributeGetter)
        {
            attributeTypes.Add(typeof(TAttribute));
            attributeGetters.Add((a, i) => attributeGetter(a)[i]);
            return this;
        }

        public ObjectBuffer<T> Build()
        {
            return new ObjectBuffer<T>(primitiveType, verticesPerObject, objectCapacity, attributeTypes, attributeGetters);
        }
    }
}
