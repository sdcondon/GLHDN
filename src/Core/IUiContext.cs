namespace OpenGlHelpers.Core
{
    using System.Collections.Generic;

    public interface IUiContext
    {
        float DisplayAspectRatio { get; }

        HashSet<char> PressedKeys { get; }

        int CursorMovementX { get; }

        int CursorMovementY { get; }

        int MouseWheelDelta { get;  }
    }
}
