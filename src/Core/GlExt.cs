using OpenGL;
using System;
using System.Diagnostics;

namespace GLHDN.Core
{
    internal static class GlExt
    {
        public static void ThrowIfNoCurrentContext()
        {
            if (DeviceContext.GetCurrentContext() == null)
            {
                throw new InvalidOperationException("Cannot do OpenGL operations because the calling thread has no current OpenGL context");
            }
        }

        [Conditional("DEBUG")]
        public static void DebugWriteLine(string message)
        {
            var method = new StackFrame(1).GetMethod();
            Debug.WriteLine(message, $"{method.DeclaringType.FullName}::{method.Name}");
        }
    }
}
