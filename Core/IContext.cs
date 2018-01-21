namespace OpenGlHelpers.Core
{
    using System.Collections.Generic;

    public interface IContext
    {
        float DisplayAspectRatio { get; }

        HashSet<char> PressedKeys { get; }

        int CursorMovementX { get; }

        int CursorMovementY { get; }
    }
}
