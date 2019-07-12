using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GLHDN.Views.Renderables.Gui
{
    public sealed class Button : ContainerElementBase
    {
        private readonly Action<Vector2> clickHandler;

        public Button(Layout layout, Color color, Color textColor, string text, Action<Vector2> clickHandler)
            : base(layout)
        {
            this.SubElements.Add(new PanelElement(Layout.Fill, color, 0)
            {
                SubElements =
                {
                    new TextElement(Layout.Fill, textColor, text)
                    {
                        HorizontalAlignment = 0.5f,
                        VerticalAlignment = 0.5f
                    }
                }
            });

            this.clickHandler = clickHandler;
        }

        protected override void OnClicked(Vector2 position)
        {
            clickHandler(position);
            base.OnClicked(position);
        }
    }
}
