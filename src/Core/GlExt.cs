using OpenGL;
using System;
using System.Diagnostics;

namespace GLHDN.Core
{
    /// <summary>
    /// Static OpenGL helper methods.
    /// </summary>
    internal static class GlExt
    {
        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if there is no OpenGL context current on the calling thread.
        /// </summary>
        public static void ThrowIfNoCurrentContext()
        {
            if (DeviceContext.GetCurrentContext() == null)
            {
                throw new InvalidOperationException("Cannot do OpenGL operations because the calling thread has no current OpenGL context");
            }
        }

        /// <summary>
        /// Records a debug message, prefixed by the calling type and method.
        /// </summary>
        /// <param name="message">The message to be recorded.</param>
        [Conditional("DEBUG")]
        public static void DebugWriteLine(string message)
        {
            var method = new StackFrame(1).GetMethod();
            Debug.WriteLine(message, $"{method.DeclaringType.FullName}::{method.Name}");
        }
    }
}
