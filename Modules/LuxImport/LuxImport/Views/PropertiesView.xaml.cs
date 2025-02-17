using Luxoria.Modules.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LuxImport.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PropertiesView : Page
    {
        private readonly IEventBus _eventBus;
        private readonly MainImportView _Parent;
        private readonly string _collectionPath;

        /// <summary>
        /// Constructor for the PropertiesView
        /// </summary>
        /// <param name="eventBus">Event bus for internal communications</param>
        /// <param name="mainImportView">Parent view</param>
        /// <param name="collectionPath">Collection path from disk</param>
        public PropertiesView(IEventBus eventBus, MainImportView mainImportView, string collectionPath)
        {
            _eventBus = eventBus;
            _Parent = mainImportView;
            _collectionPath = collectionPath;

            // Modal Properties
            Width = 500;

            this.InitializeComponent();
        }

        /// <summary>
        /// Event handler for the Cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _Parent.EntryPoint();
        }

        /// <summary>
        /// Event handler for the Create button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            string collectionName = CollectionNameTextBox.Text;

            if (string.IsNullOrWhiteSpace(collectionName))
            {
                // Show error message
                ErrorTextBlock.Visibility = Visibility.Visible;
                return;
            }

            // Hide error message if input is valid
            ErrorTextBlock.Visibility = Visibility.Collapsed;

            // Go to the indexication view
            _Parent.SetIndexicationView(collectionName, _collectionPath);
        }
    }
}
