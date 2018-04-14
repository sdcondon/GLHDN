namespace GLHDN.Views
{
    using System.Collections.Generic;

    public interface IUiContext
    {
        int DisplayWidth { get; }

        int DisplayHeight { get; }

        int CursorMovementX { get; }

        int CursorMovementY { get; }

        int MouseWheelDelta { get; }

        HashSet<char> PressedKeys { get; }

        bool MouseButtonReleased { get; }
    }
}
