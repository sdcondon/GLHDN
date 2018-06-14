namespace GLHDN.Core
{
    using OpenGL;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;

    public class BoundBuffer<TElement, TVertex> where TElement : INotifyPropertyChanged
    {
        private readonly Dictionary<TElement, int> objects = new Dictionary<TElement, int>();
        private readonly IVertexArrayObject vao;
        private readonly int verticesPerAtom;
        private readonly Func<TElement, IList> attributeGetter;
        private readonly IList<int> indices;

        private int atomCount;
        private int atomCapacity;

        public BoundBuffer(
            INotifyCollectionChanged collection,
            PrimitiveType primitiveType,
            int verticesPerAtom,
            int atomCapacity,
            Func<TElement, TVertex[]> attributeGetter,
            IList<int> indices)
        {
            this.verticesPerAtom = verticesPerAtom;
            this.attributeGetter = attributeGetter;
            this.indices = indices;
            this.atomCapacity = atomCapacity;
            this.vao = MakeVertexArrayObject(primitiveType);
            collection.CollectionChanged += Collection_CollectionChanged;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.vao.Dispose();
        }

        /// <summary>
        /// Draw with the active program.
        /// </summary>
        public void Draw()
        {
            this.vao.Draw(this.atomCount * indices.Count);
        }

        /* TODO
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
            // TODO: more graceful error handling if the item not found
            var itemIndex = this.objects[item];
            SetItemData(item, itemIndex);
        }
        */

        internal virtual IVertexArrayObject MakeVertexArrayObject(PrimitiveType primitiveType)
        {
            return new GlVertexArrayObject(
                primitiveType,
                new Func<GlVertexBufferObject>[] { () => new GlVertexBufferObject(BufferTarget.ArrayBuffer, BufferUsage.DynamicDraw, Array.CreateInstance(typeof(TVertex), atomCapacity * verticesPerAtom)) },  // TODO: different VAO ctor to avoid needless large heap allocation 
                new uint[atomCapacity * indices.Count]); // TODO: different VAO ctor to avoid needless large heap allocation
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TElement item in e.NewItems)
                    {
                        // Check buffer size
                        if (this.atomCount >= atomCapacity)
                        {
                            // TODO: expand (i.e. recreate) buffer? ResizeBuffer method in GlVertexArrayObject:
                            // Create new, glCopyBufferSubData, delete old, update buffer ID array. 
                            throw new InvalidOperationException("Buffer is full");
                        }

                        SetItemData(item, this.atomCount);
                        objects.Add(item, this.atomCount);
                        //TODO: item.PropertyChanged += Item_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    throw new NotImplementedException();
                    foreach (TElement item in e.OldItems)
                    {
                        // TODO: Find buffer segment for object (will need to keep a record).
                        // Overwrite it with last buffer segment.
                        // Don't think need to do anything with indices because of their constant nature..
                        objects.Remove(item);
                        //TODO: item.PropertyChanged -= Item_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotSupportedException();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // TODO (if/when resizing is supported): clear buffer data / shrink buffer? 
                    objects.Clear();
                    break;

            }
        }

        /// <summary>
        /// Updates attribute buffers in the underlying VAO for a given item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="atomIndex">The index of the item within the underlying VAO.</param>
        private void SetItemData(TElement item, int atomIndex)
        {
            var atomCount = 0;

            var vertices = this.attributeGetter(item);

            if (vertices.Count % this.verticesPerAtom != 0)
            {
                // TODO: and should be the same for each attribute getter..?
                throw new InvalidOperationException();
            }

            atomCount = vertices.Count / verticesPerAtom;

            for (int j = 0; j < vertices.Count; j++)
            {
                this.vao.AttributeBuffers[0][atomIndex * this.verticesPerAtom + j] = vertices[j];
            }

            this.atomCount += atomCount;

            // Update the index
            for (int i = 0; i < this.indices.Count; i++)
            {
                vao.IndexBuffer[atomIndex * this.indices.Count + i] = (uint)(atomIndex * this.verticesPerAtom + this.indices[i]);
            }
        }
    }
}
