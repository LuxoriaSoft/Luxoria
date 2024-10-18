using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using System.Diagnostics;

namespace TestModule1
{
    /// <summary>
    /// A basic module for testing purposes that interacts with the EventBus.
    /// </summary>
    public class TestModule1 : IModule
    {
        private IEventBus? _eventBus;
        private IModuleContext? _context;

        public string Name => "TestModule1";
        public string Description => "Basic module for testing purposes.";
        public string Version => "1.0.1";

        /// <summary>
        /// Initializes the module with the provided EventBus and ModuleContext.
        /// </summary>
        /// <param name="eventBus">The event bus for publishing and subscribing to events.</param>
        /// <param name="context">The context for managing module-specific data.</param>
        public void Initialize(IEventBus eventBus, IModuleContext context)
        {
            _eventBus = eventBus;
            _context = context;

            // Subscribe to the TextInputEvent to process text input
            _eventBus.Subscribe<TextInputEvent>(OnTextInputReceived);

            // Check if EventBus & Context are not null before proceeding
            if (_eventBus == null || _context == null)
            {
                Debug.WriteLine("Failed to initialize TestModule1: EventBus or Context is null");
                return;
            }

            Debug.WriteLine($"{Name} initialized");
        }

        /// <summary>
        /// Executes the module logic. This can be called to trigger specific actions.
        /// </summary>
        public void Execute()
        {
            Debug.WriteLine($"{Name} executed");
            // You can add more logic here if needed
        }

        /// <summary>
        /// Cleans up resources and subscriptions when the module is shut down.
        /// </summary>
        public void Shutdown()
        {
            // Unsubscribe from events if necessary to avoid memory leaks
            _eventBus?.Unsubscribe<TextInputEvent>(OnTextInputReceived);

            Debug.WriteLine($"{Name} shutdown");
        }

        /// <summary>
        /// Handles the TextInputEvent. This method will be called when text input is received.
        /// </summary>
        /// <param name="textInputEvent">The event containing the input text.</param>
        private void OnTextInputReceived(TextInputEvent textInputEvent)
        {
            // Process the input text
            Debug.WriteLine($"Received text: {textInputEvent.Text}");

            // Perform some processing logic with the input text (e.g., update an image)
            string updatedImagePath = ProcessInputText(textInputEvent.Text);

            // Publish an event to notify that an image has been updated
            _eventBus?.Publish(new ImageUpdatedEvent(updatedImagePath));
        }

        /// <summary>
        /// Processes the input text and generates an updated image path.
        /// </summary>
        /// <param name="inputText">The input text to process.</param>
        /// <returns>The path to the updated image.</returns>
        private string ProcessInputText(string inputText)
        {
            // Placeholder logic to simulate image processing based on input text
            // In a real scenario, this would involve actual image manipulation
            Debug.WriteLine($"Processing input text: {inputText}");

            // Return a dummy image path for demonstration purposes
            return "path/to/updated/image.png";
        }
    }
}
