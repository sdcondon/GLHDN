using System.Numerics;

namespace GLHDN.Views.Renderables.Gui
{
    public class Layout
    {
        public Layout(Dimensions parentOrigin, Dimensions localOrigin, Dimensions relativeSize)
        {
            this.ParentOrigin = parentOrigin;
            this.LocalOrigin = localOrigin;
            this.RelativeSize = relativeSize;
        }

        /// <summary>
        /// Gets the position in parent-space of the local origin.
        /// </summary>
        public Dimensions ParentOrigin { get; }

        /// <summary>
        /// Gets the position relative to the center of the element that will be placed at the parent origin.
        /// </summary>
        public Dimensions LocalOrigin { get; }

        /// <summary>
        /// Gets the size of the element in relation to its parent.
        /// </summary>
        public Dimensions RelativeSize { get;  }

        public Vector2 GetCenter(Element element)
        {
            var parentOriginScreenSpace = new Vector2(
                element.Parent.Center.X + (ParentOrigin.IsXRelative ? ParentOrigin.X * element.Parent.Size.X / 2 : ParentOrigin.X),
                element.Parent.Center.Y + (ParentOrigin.IsYRelative ? ParentOrigin.Y * element.Parent.Size.Y / 2 : ParentOrigin.Y));

            return new Vector2(
                parentOriginScreenSpace.X - (LocalOrigin.IsXRelative ? LocalOrigin.X * element.Size.X / 2 : LocalOrigin.X),
                parentOriginScreenSpace.Y - (LocalOrigin.IsYRelative ? LocalOrigin.Y * element.Size.Y / 2 : LocalOrigin.Y));
        }

        public Vector2 GetSize(Element element)
        {
            return new Vector2(
                RelativeSize.IsXRelative ? element.Parent.Size.X * RelativeSize.X : RelativeSize.X,
                RelativeSize.IsYRelative ? element.Parent.Size.Y * RelativeSize.Y : RelativeSize.Y);
        }

        public struct Dimensions
        {
            private Vector2 value;

            public Dimensions(int absoluteX, int absoluteY)
            {
                value = new Vector2(absoluteX, absoluteY);
                IsXRelative = false;
                IsYRelative = false;
            }

            public Dimensions(int absoluteX, float relativeY)
            {
                value = new Vector2(absoluteX, relativeY);
                IsXRelative = false;
                IsYRelative = true;
            }

            public Dimensions(float relativeX, int absoluteY)
            {
                value = new Vector2(relativeX, absoluteY);
                IsXRelative = true;
                IsYRelative = false;
            }

            public Dimensions(float relativeX, float relativeY)
            {
                value = new Vector2(relativeX, relativeY);
                IsXRelative = true;
                IsYRelative = true;
            }

            public float X => value.X;

            public float Y => value.Y;

            public bool IsXRelative { get; }

            public bool IsYRelative { get; }

            public static implicit operator Dimensions((int absoluteX, int absoluteY) tuple) => new Dimensions(tuple.absoluteX, tuple.absoluteY);
            public static implicit operator Dimensions((int absoluteX, float relativeY) tuple) => new Dimensions(tuple.absoluteX, tuple.relativeY);
            public static implicit operator Dimensions((float relativeX, int absoluteY) tuple) => new Dimensions(tuple.relativeX, tuple.absoluteY);
            public static implicit operator Dimensions((float relativeX, float relativeY) tuple) => new Dimensions(tuple.relativeX, tuple.relativeY);
        }
    }
}
