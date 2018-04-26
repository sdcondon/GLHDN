namespace GLHDN.Views.Renderables.Gui
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using SharpFont;

    class Font
    {
        public void Foo()
        {
            var library = new SharpFont.Library();
            var face = new Face(library, "./Fonts/Inconsolata/Inconsolata-Regular.ttf");
        }
    }
}
