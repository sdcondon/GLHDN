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
            var fillLayout = new Layout((0f, 0f), (0f, 0f), (1f, 1f));

            this.SubElements.Add(new PanelElement(fillLayout, color, 0)
            {
                SubElements =
                {
                    new TextElement(fillLayout, textColor, text)
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
