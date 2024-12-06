using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;

namespace Luxoria.Modules
{
    /// <summary>
    /// Provides a context for managing and interacting with modules.
    /// </summary>
    public class ModuleContext : IModuleContext
    {
        private ImageData _currentImage;
        private readonly object _lock = new object();

        /// <summary>
        /// Optional logging callback. If set, messages will be sent to this callback instead of the console.
        /// </summary>
        public Action<string> LogCallback { get; set; }

        /// <summary>
        /// Retrieves the current image being managed by the context.
        /// </summary>
        /// <returns>The current <see cref="ImageData"/> instance.</returns>
        public ImageData GetCurrentImage()
        {
            lock (_lock)
            {
                return _currentImage;
            }
        }

        /// <summary>
        /// Updates the current image being managed by the context.
        /// </summary>
        /// <param name="image">The new <see cref="ImageData"/> to set.</param>
        /// <exception cref="ArgumentNullException">Thrown if the provided image is null.</exception>
        public void UpdateImage(ImageData image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image), "Image cannot be null.");
            }

            lock (_lock)
            {
                _currentImage = image;
            }
        }

        /// <summary>
        /// Logs a message. If a custom logging callback is set, it will be used; otherwise, logs to the console.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <exception cref="ArgumentNullException">Thrown if the message is null.</exception>
        public void LogMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentNullException(nameof(message), "Log message cannot be null or empty.");
            }

            if (LogCallback != null)
            {
                LogCallback.Invoke(message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }
    }
}
