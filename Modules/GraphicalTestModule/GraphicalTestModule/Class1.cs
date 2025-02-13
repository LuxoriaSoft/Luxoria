using Luxoria.GModules;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GraphicalTestModule
{
    public class GraphicalTestModule : IModule, IModuleUI
    {
        private IEventBus? _eventBus;
        private IModuleContext? _context;
        private ILoggerService? _logger;

        public string Name => "Graphical Test Module";
        public string Description => "Basic module for testing purposes.";
        public string Version => "1.0.1";

        public List<ILuxMenuBarItem> Items { get; set; } = new List<ILuxMenuBarItem>();

        /// <summary>
        /// Initializes the module with the provided EventBus and ModuleContext.
        /// </summary>
        /// <param name="eventBus">The event bus for publishing and subscribing to events.</param>
        /// <param name="context">The context for managing module-specific data.</param>
        public void Initialize(IEventBus eventBus, IModuleContext context, ILoggerService logger)
        {
            _eventBus = eventBus;
            _context = context;
            _logger = logger;

            List<ISmartButton> smartButtons = new List<ISmartButton>();
            List<ISmartButton> smartButtons2 = new List<ISmartButton>();
            Dictionary<SmartButtonType, Page> Pages1 = new Dictionary<SmartButtonType, Page>();
            Dictionary<SmartButtonType, Page> Pages2 = new Dictionary<SmartButtonType, Page>();
            Dictionary<SmartButtonType, Page> Pages3 = new Dictionary<SmartButtonType, Page>();
            Dictionary<SmartButtonType, Page> Pages4 = new Dictionary<SmartButtonType, Page>();

            Pages1.Add(SmartButtonType.MainPanel, new BlankPage1());
            Pages2.Add(SmartButtonType.Window, new BlankPage1());
            Pages3.Add(SmartButtonType.Modal, new BlankPage1());

            Pages4.Add(SmartButtonType.LeftPanel, new BlankPage1());

            smartButtons.Add(new SmartButton("Main Panel", "I'm just a button of TestItem", Pages1));
            smartButtons.Add(new SmartButton("Window", "I'm just a button of TestItem", Pages2));
            smartButtons.Add(new SmartButton("Modal", "I'm just a button of TestItem", Pages3));

            smartButtons2.Add(new SmartButton("Left Panel", "I'm just a button of TestItem", Pages4));


            Items.Add(new LuxMenuBarItem("TestItem", true, new Guid(), smartButtons));

            Items.Add(new LuxMenuBarItem("TestItem2", false, new Guid(), smartButtons2));


            // Subscribe to the TextInputEvent to process text input
            _eventBus.Subscribe<TextInputEvent>(OnTextInputReceived);

            // Check if EventBus & Context are not null before proceeding
            if (_eventBus == null || _context == null)
            {
                _logger?.Log("Failed to initialize TestModule1: EventBus or Context is null", "Mods/TestModule1", LogLevel.Error);
                return;
            }

            _logger?.Log($"{Name} initialized", "Mods/TestModule1", LogLevel.Info);
        }

        /// <summary>
        /// Executes the module logic. This can be called to trigger specific actions.
        /// </summary>
        public void Execute()
        {
            _logger?.Log($"{Name} executed", "Mods/TestModule1", LogLevel.Info);
            // You can add more logic here if needed
        }

        /// <summary>
        /// Cleans up resources and subscriptions when the module is shut down.
        /// </summary>
        public void Shutdown()
        {
            // Unsubscribe from events if necessary to avoid memory leaks
            _eventBus?.Unsubscribe<TextInputEvent>(OnTextInputReceived);

            _logger?.Log($"{Name} shut down", "Mods/TestModule1", LogLevel.Info);
        }

        /// <summary>
        /// Handles the TextInputEvent. This method will be called when text input is received.
        /// </summary>
        /// <param name="textInputEvent">The event containing the input text.</param>
        private void OnTextInputReceived(TextInputEvent textInputEvent)
        {
            // Process the input text
            _logger?.Log($"Received input text: {textInputEvent.Text}", "Mods/TestModule1", LogLevel.Info);

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
            _logger?.Log($"Processing input text: {inputText}", "Mods/TestModule1", LogLevel.Info);

            // Return a dummy image path for demonstration purposes
            return "path/to/updated/image.png";
        }
    }
}
