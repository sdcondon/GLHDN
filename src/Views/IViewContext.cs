namespace GLHDN.Views
{
    using OpenGL;
    using System;
    using System.Numerics;

    public interface IViewContext
    {
        event EventHandler<DeviceContext> ContextCreated;
        event EventHandler<DeviceContext> Render;
        event EventHandler<DeviceContext> ContextUpdate;
        event EventHandler<DeviceContext> ContextDestroying;
        event EventHandler<char> KeyDown;
        event EventHandler<char> KeyUp;
        event EventHandler<int> MouseWheel;
        event EventHandler MouseUp;
        event EventHandler<Vector2> Resize;
        event EventHandler GotFocus;
        event EventHandler Update;

        /// <summary>
        /// Gets the width of the display.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the height of the display.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the position of the cursor within the context.
        /// </summary>
        Vector2 CursorPosition { get; set; }

        /// <summary>
        /// Gets a value indicating whether the context has input focus.
        /// </summary>
        bool IsFocused { get; }

        void HideCursor();
    }
}
