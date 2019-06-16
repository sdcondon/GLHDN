namespace GLHDN.Views
{
    using OpenGL;
    using System;
    using System.Numerics;

    /// <summary>
    /// Interface for types that provide the mechanism for <see cref="View"/> instances to interact with the user and the Open GL device context.
    /// </summary>
    public interface IViewContext
    {
        event EventHandler<DeviceContext> GlContextCreated;
        event EventHandler<DeviceContext> GlRender;
        event EventHandler<DeviceContext> GlContextUpdate;
        event EventHandler<DeviceContext> GlContextDestroying;
        event EventHandler<char> KeyDown;
        event EventHandler<char> KeyUp;
        event EventHandler LeftMouseDown;
        event EventHandler LeftMouseUp;
        event EventHandler RightMouseDown;
        event EventHandler RightMouseUp;
        event EventHandler MiddleMouseDown;
        event EventHandler MiddleMouseUp;
        event EventHandler<int> MouseWheel;
        event EventHandler<Vector2> Resize;
        event EventHandler GotFocus;

        /// <summary>
        /// Gets the width of the display.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the height of the display.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets a value indicating whether the context has input focus.
        /// </summary>
        bool IsFocused { get; }

        /// <summary>
        /// Gets or sets the position of the mouse cursor within the context.
        /// </summary>
        Vector2 CursorPosition { get; set; }

        /// <summary>
        /// Instructs the context to hide the mouse cursor.
        /// </summary>
        void HideCursor();
    }
}
