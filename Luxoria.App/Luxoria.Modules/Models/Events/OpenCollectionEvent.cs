using Luxoria.Modules.Interfaces;

namespace Luxoria.Modules.Models.Events
{
    public class OpenCollectionEvent
    {
        // Path to the collection
        public string Path { get; set; }

        // Event for progress messages
        public event OnProgressMessage? ProgressMessage;

        // Delegate for the progress message event
        public delegate void OnProgressMessage(string message, int? progress);

        // Constructor
        public OpenCollectionEvent(string path)
        {
            // Set the path
            Path = path;
        }

        /// <summary>
        /// Sends a progress message with a message and an optional progress value.
        /// </summary>
        /// <param name="message">Message to be sent through the event channel.</param>
        /// <param name="progressValue">Optional progress value (0-100).</param>
        public void SendProgressMessage(string message, int? progressValue = null)
        {
            // Invoke the ProgressMessage event if there are subscribers
            ProgressMessage?.Invoke(message, progressValue);
        }

        /// <summary>
        /// Completes the channel to signal no more messages will be sent.
        /// </summary>
        public void Complete()
        {
            // Logic to complete the event (optional)
        }
    }
}
