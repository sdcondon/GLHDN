using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GLHDN.Views.Renderables.Gui
{
    public sealed class Button : ContainerElementBase
    {
        private readonly Action<Vector2> clickHandler;
        private readonly Panel panel;

        public Button(Layout layout, Color color, Color textColor, string text, Action<Vector2> clickHandler)
            : base(layout)
        {
            this.SubElements.Add(this.panel = new Panel(Layout.Fill, color)
            {
                SubElements =
                {
                    new Text(Layout.Fill, textColor, text)
                    {
                        HorizontalAlignment = 0.5f,
                        VerticalAlignment = 0.5f
                    }
                }
            });

            this.clickHandler = clickHandler;
        }

        public Vector4 Color
        {
            get => this.panel.Color;
            set => this.panel.Color = value;
        }

        protected override void OnClicked(Vector2 position)
        {
            clickHandler(position);
            base.OnClicked(position);
        }
    }
}
