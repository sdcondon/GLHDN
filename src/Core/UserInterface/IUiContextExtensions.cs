namespace OpenGlHelpers.Core
{
    using System.Collections.Generic;

    public static class IUiContextExtensions
    {
        public static float DisplayAspectRatio (this IUiContext uiContext)
        {
            return (float)uiContext.DisplayWidth / (float)uiContext.DisplayHeight;
        }
    }
}
