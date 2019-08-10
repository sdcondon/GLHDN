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
        /// <summary>Occurs when an OpenGL context has been created.</summary>
        event EventHandler<DeviceContext> GlContextCreated;

        /// <summary>Occurs when the context is rendering.</summary>
        event EventHandler<DeviceContext> GlRender;

        /// <summary>Occurs when the context is updating.</summary>
        event EventHandler<DeviceContext> GlContextUpdate;

        /// <summary>Occurs when the OpenGL is being destroyed.</summary>
        event EventHandler<DeviceContext> GlContextDestroying;

        /// <summary>Occurs when a key is pressed is pressed.</summary>
        event EventHandler<char> KeyDown;

        /// <summary>Occurs when a key is released.</summary>
        event EventHandler<char> KeyUp;

        /// <summary>Occurs when the left mouse button is pressed.</summary>
        event EventHandler LeftMouseDown;

        /// <summary>Occurs when the left mouse button is released.</summary>
        event EventHandler LeftMouseUp;

        /// <summary>Occurs when the right mouse button is pressed.</summary>
        event EventHandler RightMouseDown;

        /// <summary>Occurs when the right mouse button is released.</summary>
        event EventHandler RightMouseUp;

        /// <summary>Occurs when the middle mouse button is pressed.</summary>
        event EventHandler MiddleMouseDown;

        /// <summary>Occurs when the middle mouse button is released.</summary>
        event EventHandler MiddleMouseUp;

        /// <summary>Occurs when the mouse wheel is used.</summary>
        event EventHandler<int> MouseWheel;

        /// <summary>Occurs when the context is resized.</summary>
        event EventHandler<Vector2> Resize;

        /// <summary>Occurs when the context receives input focus.</summary>
        event EventHandler GotFocus;

        /// <summary>Gets the width of the display.</summary>
        int Width { get; }

        /// <summary>Gets the height of the display.</summary>
        int Height { get; }

        /// <summary>Gets a value indicating whether the context has input focus.</summary>
        bool IsFocused { get; }

        /// <summary>Gets or sets the position of the mouse cursor within the context.</summary>
        Vector2 CursorPosition { get; set; }

        /// <summary>Sets a value indicating whether the mouse cursor should be displayed.</summary>
        bool ShowCursor { set; }

        /// <summary>Closes the app.</summary>
        void Exit();
    }
}
