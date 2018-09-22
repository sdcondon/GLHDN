namespace GLHDN.Views.Renderables.Gui
{
    using System.ComponentModel;
    using System.Numerics;

    internal class RootElement : GuiElement
    {
        private View view;

        public RootElement(View view)
        {
            this.view = view;
        }

        /// <inheritdoc />
        public Vector2 Center_ScreenSpace => Vector2.Zero;

        /// <inheritdoc />
        public Vector2 Size_ScreenSpace => new Vector2(view.Width, view.Height);

        /// <inheritdoc />
        public override GuiVertex[] Vertices { get; } = new GuiVertex[0];

        public override event PropertyChangedEventHandler PropertyChanged;
    }
}
