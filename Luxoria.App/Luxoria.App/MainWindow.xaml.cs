using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Microsoft.UI.Xaml;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Luxoria.App
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly IEventBus _eventBus;

        public MainWindow(IEventBus eventBus)
        {
            this.InitializeComponent();
            _eventBus = eventBus;
            Initialize();

        }

        public void Initialize()
        {
            // Subscribe to the ImageUpdatedEvent
            _eventBus.Subscribe<ImageUpdatedEvent>(OnImageUpdated);
        }

        private void SendToModule_Click(object sender, RoutedEventArgs e)
        {
            string inputText = InputTextBox.Text;
            Log($"Sending input text: {inputText}");

            // Publish the input text to the module
            _eventBus.Publish(new TextInputEvent(inputText));

            // Optionally, clear the TextBox after sending
            InputTextBox.Text = string.Empty;
        }

        private void OnImageUpdated(ImageUpdatedEvent imageUpdatedEvent)
        {
            // Handle the response from the module
            // For example, display the updated image or log a message
            Log($"Image updated: {imageUpdatedEvent.ImagePath}");
        }

        private void Log(string message)
        {
            // Log the message (e.g., output to console or a log file)
            Debug.WriteLine(message);
        }
    }
}
