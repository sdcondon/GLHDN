namespace GLHDN.Views.Renderables.Gui
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// 
    /// </summary>
    public class PanelElement : Element, IElementParent
    {
        public PanelElement()
        {
            SubElements = new SubElementCollection(this);
        }

        /// <summary>
        /// Gets or sets the background color of the element.
        /// </summary>
        public Vector4 Color { get; set; }

        /// <summary>
        /// Gets or sets the width of the border of the element.
        /// </summary>
        public float BorderWidth { get; set; }

        /// <inheritdoc />
        public override GuiVertex[] Vertices => new[]
        {
            new GuiVertex(PosTL, Color, PosBL, Size, BorderWidth),
            new GuiVertex(PosTR, Color, PosBL, Size, BorderWidth),
            new GuiVertex(PosBL, Color, PosBL, Size, BorderWidth),
            new GuiVertex(PosBR, Color, PosBL, Size, BorderWidth)
        };

        public override event PropertyChangedEventHandler PropertyChanged;

        // TODO: handlers? - onclick, onmouseover, onmouseout etc

        public ICollection<Element> SubElements { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Ultimately to work nicely with our BoundBuffer class, we need to flatten the elements in the single collection
        /// in the <see cref="Gui"/> instance. So that collection ultimately backs this one, which just provides a view on
        /// it consisting of all the elements with a particular parent element.
        /// </remarks>
        private class SubElementCollection : ICollection<Element>
        {
            private PanelElement owner;

            public SubElementCollection(PanelElement owner)
            {
                this.owner = owner;
            }

            public int Count => throw new System.NotImplementedException();

            public bool IsReadOnly => throw new System.NotImplementedException();

            public void Add(Element element)
            {
                element.Parent = element.Parent ?? this.owner;
                owner.Parent.SubElements.Add(element);
            }

            public bool Remove(Element element)
            {
                return owner.Parent.SubElements.Remove(element);
            }

            public void Clear()
            {
                owner.Parent.SubElements.Clear(); // todo only this
            }

            public bool Contains(Element item)
            {
                throw new System.NotImplementedException();
            }

            public void CopyTo(Element[] array, int arrayIndex)
            {
                throw new System.NotImplementedException();
            }

            /// <inheritdoc />
            public IEnumerator<Element> GetEnumerator()
            {
                return owner.Parent.SubElements
                    .Where(e => e.Parent == this.owner)
                    .GetEnumerator();
            }

            /// <inheritdoc />)
            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)owner.Parent.SubElements
                    .Where(e => e.Parent == this.owner))
                    .GetEnumerator();
            }
        }
    }
}
