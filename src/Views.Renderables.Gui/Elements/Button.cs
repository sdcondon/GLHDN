﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GLHDN.Views.Renderables.Gui
{
    public sealed class Button : ElementBase, IElementParent
    {
        private readonly PanelElement panel;

        public Button(Layout layout, Color color, Color textColor, string text, EventHandler<Vector2> onClick)
            : base(layout)
        {
            var fillLayout = new Layout((0f, 0f), (0f, 0f), (1f, 1f));

            this.SubElements = new ElementCollection(this)
            {
                new PanelElement(fillLayout, color, 0)
                {
                    SubElements =
                    {
                        new TextElement(fillLayout, textColor, text)
                    }
                }
            };

            this.Clicked += onClick;
        }

        public override IList<Vertex> Vertices { get; } = new Vertex[0];

        public ElementCollection SubElements { get; }
    }
}