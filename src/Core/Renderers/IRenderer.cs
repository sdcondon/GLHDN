namespace OpenGlHelpers.Core
{
    public interface IRenderer
    {
        /// <summary>
        /// Handler for render context creation.
        /// </summary>
        /// <param name="sender"></param>
        void ContextCreated(object sender);

        /// <summary>
        /// Render logic.
        /// </summary>
        /// <param name="sender"></param>
        void Render(object sender);

        /// <summary>
        /// Handles context updates.
        /// </summary>
        /// <param name="sender"></param>
        void ContextUpdate(object sender);

        /// <summary>
        /// Handler for the render context being destroyed.
        /// </summary>
        /// <param name="sender"></param>
        void ContextDestroying(object sender);
    }
}
